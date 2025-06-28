using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using Unity.Mathematics;
using VelorenPort.CoreEngine;
using VelorenPort.Network;
using VelorenPort.World;
using VelorenPort.Server.Sys;
using VelorenPort.Server.Settings;

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
        private readonly Persistence _persistence = new();
        private readonly Channel<SerializedChunk> _chunkChannel = Channel.CreateUnbounded<SerializedChunk>();
        private readonly NetworkRequestMetrics _networkMetrics = new();
        private readonly ChunkSerialize _chunkSerialize = new();
        private readonly InviteManager _inviteManager;
        private readonly Chat.ChatCache _chatCache;
        private readonly Chat.ChatExporter _chatExporter;
        private readonly Events.EventManager _eventManager = new();
        private readonly Weather.WeatherJob _weatherJob = new();
        private ulong _tick;

        /// <summary>Returns the connected clients.</summary>
        public IEnumerable<Client> Clients => _clients;
        public CharacterUpdater CharacterUpdater => _characterUpdater;
        public Events.EventManager Events => _eventManager;

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
        }

        /// <summary>
        /// Listen for connections and start the main loop until the
        /// provided cancellation token is signalled.
        /// </summary>
        public async Task RunAsync(ListenAddr addr, CancellationToken token) {
            var connectionTask = _connections.RunAsync(addr, token);
            while (!token.IsCancellationRequested) {
                Clock.Tick();
                AcceptNewClients();
                UpdateWorld();
                _chunkSerialize.Flush(WorldIndex, _chunkChannel, _networkMetrics);
                await ChunkSend.FlushAsync(_chunkChannel, _clients, _networkMetrics);
                await EntitySync.BroadcastAsync(_clients);
                _terrainPersistence.Maintain();
                _persistence.Update(_tick, _characterUpdater, _terrainPersistence);
                _metrics.RecordTick();
                _infoBroadcaster.Update(_tick++, _settings, _clients.Count);
                await Task.Yield();
            }
            await connectionTask;
            _terrainPersistence.Dispose();
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
            ChatSystem.Update(_eventManager, _chatExporter, _clients);
            WeatherSystem.Update(WorldIndex, _weatherJob, _clients);
        }

        private void OnServerInfo(ServerInfo info) {
            var msg = PreparedMsg.Create(0, info, new StreamParams(Promises.Ordered));
            var tasks = new List<Task>();
            foreach (var client in _clients) {
                tasks.Add(client.SendPreparedAsync(msg));
            }
            Task.WhenAll(tasks).GetAwaiter().GetResult();
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
