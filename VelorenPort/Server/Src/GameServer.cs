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

namespace VelorenPort.Server {
    /// <summary>
    /// Coordinates the main game loop and manages connected clients. This
    /// version aims to mirror the high level behaviour of the Rust server
    /// crate using idiomatic C# constructs.
    /// </summary>
    public class GameServer {
        public Network.Network Network { get; }
        public Clock Clock { get; }
        public WorldIndex WorldIndex { get; }

        private readonly List<Client> _clients = new();
        private readonly ConnectionHandler _connections;
        private readonly Metrics _metrics = new();
        private readonly ServerInfoBroadcaster _infoBroadcaster;
        private readonly Settings.Settings _settings;
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
        private readonly List<Teleporter> _teleporters = new();
        private readonly List<NpcSpawnerSystem.SpawnPoint> _npcSpawnPoints = new();
        private readonly SentinelSystem.Trackers _sentinelTrackers = new();
        private readonly Ecs.Dispatcher _dispatcher = new();
        private QueryServer? _queryServer;
        private ulong _tick;

        /// <summary>Returns the connected clients.</summary>
        public IEnumerable<Client> Clients => _clients;
        public CharacterUpdater CharacterUpdater => _characterUpdater;
        public Persistence.CharacterLoader CharacterLoader => _characterLoader;
        public Events.EventManager Events => _eventManager;
        public Plugin.PluginManager Plugins => _pluginManager;
        public VelorenPort.CoreEngine.comp.GroupManager GroupManager => _groupManager;

        public GameServer(Pid pid, TimeSpan tickRate, uint worldSeed) {
            Network = new Network.Network(pid);
            Clock = new Clock(tickRate);
            WorldIndex = new WorldIndex(worldSeed);
            _connections = new ConnectionHandler(Network);
            _settings = new Settings.Settings();
            _terrainPersistence = new TerrainPersistence(DataDir.DefaultDataDirName);
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

            _dispatcher.AddSystem(new DelegateSystem(dt => {
                InviteTimeout.Update(_clients);
                ChatSystem.Update(_eventManager, _chatExporter, _autoMod, _clients);
                WeatherSystem.Update(WorldIndex, _weatherJob, _clients);
                TeleporterSystem.Update(_clients, _teleporters);
                PortalSystem.Update(WorldIndex.EntityManager, _clients, dt);
                NpcSpawnerSystem.Update(WorldIndex.EntityManager, _npcSpawnPoints, dt);
                NpcAiSystem.Update(WorldIndex.EntityManager, _clients, dt);
                PetsSystem.Update(WorldIndex.EntityManager, _clients, dt);
                LootSystem.Update(WorldIndex.EntityManager);
                ObjectSystem.Update(WorldIndex.EntityManager);
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
                        client.SendPreparedAsync(msg).GetAwaiter().GetResult();
                }
            }));
        }

        /// <summary>
        /// Listen for connections and start the main loop until the
        /// provided cancellation token is signalled.
        /// </summary>
        public async Task RunAsync(ListenAddr addr, CancellationToken token) {
            var connectionTask = _connections.RunAsync(addr, token);
            CancellationTokenSource? queryCts = null;
            Task? queryTask = null;
            if (_settings.EnableQueryServer) {
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
            while (!token.IsCancellationRequested) {
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
            await connectionTask;
            _terrainPersistence.Dispose();
            _metricsExporter.Dispose();
        }

        private void AcceptNewClients() {
            while (_connections.TryDequeue(out var client)) {
                _clients.Add(client);
            }
        }

        private void UpdateWorld() {
            WorldIndex.Time += (float)Clock.Dt.TotalSeconds;

            foreach (var client in _clients) {
                RegionSubscriptionUpdater.UpdateSubscription(
                    client.Position,
                    client.Presence,
                    client.RegionSubscription);

                TerrainSync.Update(WorldIndex, client, _chunkSerialize);
            }

            InviteTimeout.Update(_clients);
            ChatSystem.Update(_eventManager, _chatExporter, _autoMod, _clients, _groupManager);
            WeatherSystem.Update(WorldIndex, _weatherJob, _clients);
            TeleporterSystem.Update(_clients, _teleporters);
            PortalSystem.Update(WorldIndex.EntityManager, _clients, (float)Clock.Dt.TotalSeconds);
            NpcSpawnerSystem.Update(WorldIndex.EntityManager, _npcSpawnPoints, (float)Clock.Dt.TotalSeconds);
            NpcAiSystem.Update(WorldIndex.EntityManager, _clients, (float)Clock.Dt.TotalSeconds);
            PetsSystem.Update(WorldIndex.EntityManager);
            LootSystem.Update(WorldIndex.EntityManager);
            ObjectSystem.Update(WorldIndex.EntityManager);
            WiringSystem.Update(WorldIndex.EntityManager);
            SentinelSystem.Update(WorldIndex.EntityManager, _sentinelTrackers);

            var groupEvents = _groupManager.Events.RecvAll();
            if (groupEvents.Count > 0)
            {
                foreach (var ev in groupEvents)
                {
                    var msg = PreparedMsg.Create(
                        0,
                        new ServerGeneral.GroupUpdate(ev),
                        new StreamParams(Promises.Ordered));
                    foreach (var client in _clients)
                        client.SendPreparedAsync(msg).GetAwaiter().GetResult();
                }
            }
            _dispatcher.Update((float)Clock.Dt.TotalSeconds);
        }

        private void OnServerInfo(ServerInfo info) {
            var msg = PreparedMsg.Create(0, info, new StreamParams(Promises.Ordered));
            var tasks = new List<Task>();
            foreach (var client in _clients) {
                tasks.Add(client.SendPreparedAsync(msg));
            }
            Task.WhenAll(tasks).GetAwaiter().GetResult();
            _queryServer?.UpdateInfo(info);
        }

        /// <summary>
        /// Sends a message to all connected clients. Currently this just logs
        /// to the console as networking has not been fully implemented.
        /// </summary>
        public void NotifyPlayers(string msg) {
            Console.WriteLine(msg);
        }

        public void SendInvite(Uid inviter, Uid invitee, InviteKind kind) =>
            _inviteManager.SendInvite(inviter, invitee, kind);

        public void RespondToInvite(Uid invitee, Uid inviter, InviteKind kind, InviteAnswer answer) =>
            _inviteManager.HandleResponse(invitee, inviter, kind, answer);

        /// <summary>Returns simple identifiers for all connected clients.</summary>
        public IEnumerable<string> GetOnlinePlayerNames() =>
            _clients.Select(c => c.Participant.Id.Value.ToString());
    }
}
