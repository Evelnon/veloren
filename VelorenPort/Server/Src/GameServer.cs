using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using VelorenPort.NativeMath;
using System.Net;
using VelorenPort.CoreEngine;
using VelorenPort.Network;
using VelorenPort.World;
using VelorenPort.Server.Sys;
using VelorenPort.Server.Settings;
using VelorenPort.Server.Ecs;
using VelorenPort.Server.Weather;
using VelorenPort.Server.Rtsim;

namespace VelorenPort.Server
{
    /// <summary>
    /// Coordinates the main game loop and manages connected clients. This
    /// version aims to mirror the high level behaviour of the Rust server
    /// crate using idiomatic C# constructs.
    /// </summary>
    public class GameServer
    {
        public Network.Network Network { get; }
        public Clock Clock { get; }
        public WorldIndex WorldIndex { get; }

        private readonly List<Client> _clients = new();
        private readonly ConnectionHandler _connections;
        private readonly Metrics _metrics = new();
        private readonly ServerInfoBroadcaster _infoBroadcaster;
        private Settings.Settings _settings;
        private readonly TerrainPersistence _terrainPersistence;
        private readonly CharacterUpdater _characterUpdater = new();
        private readonly Persistence.CharacterLoader _characterLoader = new();
        private readonly Persistence _persistence = new();
        private readonly Channel<SerializedChunk> _chunkChannel = Channel.CreateUnbounded<SerializedChunk>();
        private readonly NetworkRequestMetrics _networkMetrics = new();
        private readonly ChunkSerialize _chunkSerialize = new();
        private readonly InviteManager _inviteManager;
        private readonly VelorenPort.CoreEngine.comp.GroupManager _groupManager = new();
        private readonly PrometheusExporter _metricsExporter;
        private readonly Chat.ChatCache _chatCache;
        private readonly Chat.ChatExporter _chatExporter;
        private readonly Events.EventManager _eventManager = new();
        private readonly Plugin.PluginManager _pluginManager = new();
        private readonly AutoMod _autoMod;
        private readonly Weather.WeatherJob _weatherJob = new();
        private readonly Weather.WeatherSim _weatherSim;
        private readonly Rtsim.RtSim _rtsim = new();
        private readonly List<Teleporter> _teleporters = new();
        private readonly List<NpcSpawnerSystem.SpawnPoint> _npcSpawnPoints = new();
        private readonly SentinelSystem.Trackers _sentinelTrackers = new();
        private readonly Ecs.Dispatcher _dispatcher;
        private QueryServer? _queryServer;
        private QueryClient? _discoveryClient;
        private ulong _tick;

        /// <summary>Returns the connected clients.</summary>
        public IEnumerable<Client> Clients => _clients;
        public CharacterUpdater CharacterUpdater => _characterUpdater;
        public Persistence.CharacterLoader CharacterLoader => _characterLoader;
        public Events.EventManager Events => _eventManager;
        public Plugin.PluginManager Plugins => _pluginManager;
        public VelorenPort.CoreEngine.comp.GroupManager GroupManager => _groupManager;

        public GameServer(Pid pid, TimeSpan tickRate, uint worldSeed)
        {
            Network = new Network.Network(pid);
            Clock = new Clock(tickRate);
            WorldIndex = new WorldIndex(worldSeed);
            _connections = new ConnectionHandler(Network);
            _settings = new Settings.Settings();
            _dispatcher = new Ecs.Dispatcher(_settings.DispatcherWorkers);
            _terrainPersistence = new TerrainPersistence(
                DataDir.DefaultDataDirName,
                _settings.TerrainArchiveLimit);
            _infoBroadcaster = new ServerInfoBroadcaster(OnServerInfo);
            _inviteManager = new InviteManager(this);
            (_chatCache, _chatExporter) = Chat.ChatCache.Create(TimeSpan.FromMinutes(1));
            var modSettings = new ModerationSettings { Automod = true };
            var banned = modSettings.LoadBannedWords(DataDir.DefaultDataDirName);
            _autoMod = new AutoMod(modSettings, new Censor(banned));
            _characterUpdater.LoadAll();
            _characterLoader.LoadAll();
            _metricsExporter = new PrometheusExporter(_metrics);
            _metricsExporter.Start();
            var pluginDir = System.IO.Path.Combine(DataDir.DefaultDataDirName, "plugins");
            _pluginManager.LoadDirectory(pluginDir);
            _teleporters.Add(new Teleporter { Position = new float3(0, 0, 0), Target = new float3(50, 50, 10) });
            _teleporters.Add(new Teleporter { Position = new float3(50, 50, 10), Target = new float3(0, 0, 0) });
            _npcSpawnPoints.Add(new NpcSpawnerSystem.SpawnPoint
            {
                Position = new float3(10, 10, 0),
                Interval = 5f,
                Timer = 0f,
                MaxNpcs = 3
            });

            _weatherSim = new Weather.WeatherSim(new int2(1, 1), worldSeed);
            _weatherJob.WeatherChanged += _weatherSim.ApplyGlobalWeather;

            _rtsim.AddRule(new Rtsim.Rule.DepleteResources());

            var weatherSystem = new WeatherTickSystem(WorldIndex, _weatherJob, _weatherSim, _clients);
            var rtsimSystem = new Rtsim.TickSystem(_rtsim);
            var gameplaySystem = new DelegateSystem((dt, ev) =>
            {
                InviteTimeout.Update(_clients);
                ChatSystem.Update(ev, _chatExporter, _autoMod, _clients, _groupManager);
                WeatherSystem.Update(WorldIndex, _weatherJob, _clients);
                TeleporterSystem.Update(_clients, _teleporters, ev);
                TeleportEventSystem.Update(ev, _clients);

                PortalSystem.Update(WorldIndex.EntityManager, _clients, dt);
                NpcSpawnerSystem.Update(WorldIndex.EntityManager, _npcSpawnPoints, dt);
                NpcAiSystem.Update(WorldIndex.EntityManager, _clients, dt, WorldIndex.Time);
                PetsSystem.Update(WorldIndex.EntityManager, _clients, dt);
                LootSystem.Update(ev, WorldIndex.EntityManager);
                ObjectSystem.Update(ev, WorldIndex.EntityManager);
                WiringSystem.Update(WorldIndex.EntityManager);
                SentinelSystem.Update(WorldIndex.EntityManager, _sentinelTrackers);

                var groupEvents = _groupManager.Events.RecvAll();
                foreach (var ev in groupEvents)
                {
                    var msg = PreparedMsg.Create(
                        0,
                        new ServerGeneral.GroupUpdate(ev),
                        new StreamParams(Promises.Ordered));
                    foreach (var client in _clients)
                    {
                        client.SendPreparedAsync(msg).GetAwaiter().GetResult();
                        client.Participant.NotifyGroupUpdate(ev);
                    }
                }

                var privEvents = _groupManager.PrivilegeEvents.RecvAll();
                foreach (var ev in privEvents)
                {
                    var msg = PreparedMsg.Create(
                        0,
                        new ServerGeneral.GroupPrivilegeUpdate(ev.Group, ev.Member, ev.Privileges),
                        new StreamParams(Promises.Ordered));
                    foreach (var client in _clients)
                    {
                        client.SendPreparedAsync(msg).GetAwaiter().GetResult();
                        client.Participant.NotifyGroupPrivilegeUpdate(ev.Group, ev.Member, ev.Privileges);
                    }
                }
            });

            _dispatcher.AddSystem(weatherSystem);
            _dispatcher.AddSystem(rtsimSystem);
            _dispatcher.AddSystem(gameplaySystem, typeof(WeatherTickSystem));
        }

        /// <summary>
        /// Listen for connections and start the main loop until the
        /// provided cancellation token is signalled.
        /// </summary>
        public async Task RunAsync(ListenAddr addr, CancellationToken token)
        {
            var connectionTask = _connections.RunAsync(addr, token);
            CancellationTokenSource? queryCts = null;
            Task? queryTask = null;
            CancellationTokenSource? discoveryCts = null;
            Task? discoveryTask = null;
            if (_settings.EnableQueryServer)
            {
                queryCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                var info = new ServerInfo(
                    GitInfo.Hash,
                    GitInfo.Timestamp,
                    0,
                    (ushort)_settings.MaxPlayers,
                    BattleMode.PvE);
                _queryServer = new QueryServer(
                    new IPEndPoint(IPAddress.Loopback, _settings.QueryServerPort),
                    info,
                    _settings.QueryServerRatelimit);
                queryTask = _queryServer.RunAsync(queryCts.Token);
            }
            if (_settings.EnableDiscovery)
            {
                discoveryCts = CancellationTokenSource.CreateLinkedTokenSource(token);
                _discoveryClient = new QueryClient(ParseEndpoint(_settings.DiscoveryAddress));
                discoveryTask = DiscoveryLoopAsync(_discoveryClient, discoveryCts.Token);
            }
            while (!token.IsCancellationRequested)
            {
                Clock.Tick();
                AcceptNewClients();
                UpdateWorld();
                _chunkSerialize.Flush(WorldIndex, _chunkChannel, _networkMetrics);
                await ChunkSend.FlushAsync(_chunkChannel, _clients, _networkMetrics);
                await EntitySync.BroadcastAsync(_clients, WorldIndex.EntityManager);
                _terrainPersistence.Maintain();
                _persistence.Update(_tick, _characterUpdater, _terrainPersistence, _characterLoader);
                _metrics.RecordTick();
                _infoBroadcaster.Update(_tick++, _settings, _clients.Count);
                await Task.Yield();
            }
            if (queryCts != null) queryCts.Cancel();
            if (queryTask != null) await queryTask;
            if (discoveryCts != null) discoveryCts.Cancel();
            if (discoveryTask != null) await discoveryTask;
            await connectionTask;
            _terrainPersistence.Dispose();
            _metricsExporter.Dispose();
        }

        private void AcceptNewClients()
        {
            while (_connections.TryDequeue(out var client))
            {
                _clients.Add(client);
            }
        }

        private void UpdateWorld()
        {
            WorldIndex.Time += (float)Clock.Dt.TotalSeconds;

            foreach (var client in _clients)
            {
                RegionSubscriptionUpdater.UpdateSubscription(
                    client.Position,
                    client.Presence,
                    client.RegionSubscription);

                TerrainSync.Update(WorldIndex, client, _chunkSerialize, _npcSpawnPoints, _rtsim);
            }

            _dispatcher.Update((float)Clock.Dt.TotalSeconds, _eventManager);
#if DEBUG
            _eventManager.DebugCheckAllConsumed();
#endif
        }

        private void OnServerInfo(ServerInfo info)
        {
            var msg = PreparedMsg.Create(0, info, new StreamParams(Promises.Ordered));
            var tasks = new List<Task>();
            foreach (var client in _clients)
            {
                tasks.Add(client.SendPreparedAsync(msg));
            }
            Task.WhenAll(tasks).GetAwaiter().GetResult();
            _queryServer?.UpdateInfo(info);
        }

        /// <summary>
        /// Sends a message to all connected clients. Currently this just logs
        /// to the console as networking has not been fully implemented.
        /// </summary>
        public void NotifyPlayers(string msg)
        {
            Console.WriteLine(msg);
        }

        /// <summary>Ban a player by username.</summary>
        public void BanPlayer(string username, string reason)
        {
            var uuid = LoginProvider.DeriveUuid(username);
            _settings.Banlist.BanUuid(uuid, username, new Settings.Banlist.BanInfo { Reason = reason }, null);
            _settings.Banlist.Save(System.IO.Path.Combine(DataDir.DefaultDataDirName, "banlist.json"));
        }

        /// <summary>Remove an existing ban for a player.</summary>
        public void UnbanPlayer(string username)
        {
            var uuid = LoginProvider.DeriveUuid(username);
            _settings.Banlist.UnbanUuid(uuid, username, new Settings.Banlist.BanInfo());
            _settings.Banlist.Save(System.IO.Path.Combine(DataDir.DefaultDataDirName, "banlist.json"));
        }

        /// <summary>Get simple server statistics.</summary>
        public string GetStats() => $"players={_clients.Count} ticks={_metrics.Ticks}";

        /// <summary>Reload configuration files from disk.</summary>
        public void ReloadConfiguration()
        {
            _settings = Settings.Settings.Load(DataDir.DefaultDataDirName);
            _terrainPersistence.RotationLimit = _settings.TerrainArchiveLimit;
        }

        /// <summary>Enumerate current bans for CLI display.</summary>
        public IEnumerable<string> BanListEntries()
        {
            foreach (var (uuid, entry) in _settings.Banlist.UuidBans())
            {
                var info = entry.Current.Action.AsBan()?.GetInfo();
                var reason = info?.Reason ?? string.Empty;
                yield return $"{uuid}:{reason}";
            }
        }

        public void SendInvite(Uid inviter, Uid invitee, InviteKind kind) =>
            _inviteManager.SendInvite(inviter, invitee, kind);

        public void RespondToInvite(Uid invitee, Uid inviter, InviteKind kind, InviteAnswer answer) =>
            _inviteManager.HandleResponse(invitee, inviter, kind, answer);

        /// <summary>Returns simple identifiers for all connected clients.</summary>
        public IEnumerable<string> GetOnlinePlayerNames() =>
            _clients.Select(c => c.Participant.Id.Value.ToString());

        static IPEndPoint ParseEndpoint(string str)
        {
            if (!str.Contains("://")) str = "udp://" + str;
            var uri = new Uri(str);
            var addresses = Dns.GetHostAddresses(uri.Host);
            return new IPEndPoint(addresses[0], uri.Port);
        }

        static async Task DiscoveryLoopAsync(QueryClient client, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await client.ServerInfoAsync();
                }
                catch { }
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                }
                catch { break; }
            }
        }
    }
}
