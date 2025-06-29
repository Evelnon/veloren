
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VelorenPort.NativeMath;
using VelorenPort.Network;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;

namespace VelorenPort.Server {
    /// <summary>
    /// Represents a connected game client with its participant handle.
    /// Additional per-client state will be added as systems are ported.
    /// </summary>
    public class Client : IDamageable {
        public Participant Participant { get; }
        public Uid Uid { get; }
        public Pos Position { get; private set; }
        public Ori Orientation { get; private set; } = Ori.Identity;
        Uid IDamageable.Id => Uid;
        public float Health { get; set; } = 100f;
        public Presence Presence { get; }
        public RegionSubscription RegionSubscription { get; }
        public ConnectAddr ConnectedFromAddr { get; }
        private readonly Dictionary<byte, Stream> _streams = new();

        private static readonly StreamParams RegisterParams = new(
            Promises.Ordered | Promises.Consistency | Promises.Compressed,
            priority: 3,
            guaranteedBandwidth: 500);

        private static readonly StreamParams CharacterScreenParams = RegisterParams;

        private static readonly StreamParams InGameParams = new(
            Promises.Ordered | Promises.Consistency | Promises.Compressed,
            priority: 3,
            guaranteedBandwidth: 100_000);

        private static readonly StreamParams GeneralParams = RegisterParams;

        private static readonly StreamParams PingParams = new(
            Promises.Ordered | Promises.Consistency,
            priority: 2,
            guaranteedBandwidth: 500);

        private static readonly StreamParams TerrainParams = new(
            Promises.Ordered | Promises.Consistency,
            priority: 4,
            guaranteedBandwidth: 20_000);
        public HashSet<int2> LoadedChunks { get; } = new();
        public PendingInvites PendingInvites { get; } = new();
        public Waypoint? Waypoint { get; set; }

        private Client(Participant participant) {
            Participant = participant;
            Uid = new Uid((ulong)participant.Id.Value);
            ConnectedFromAddr = participant.ConnectedFrom;
            Position = new Pos(float3.zero);
            Orientation = Ori.Identity;
            Presence = new Presence(new ViewDistances(8, 8), new PresenceKind.Spectator());
            RegionSubscription = RegionUtils.InitializeRegionSubscription(Position, Presence);
        }

        internal static async Task<Client> CreateAsync(Participant participant) {
            var client = new Client(participant);
            await client.InitializeStreamsAsync();
            return client;
        }

        private async Task InitializeStreamsAsync() {
            _streams[0] = await Participant.OpenStreamAsync(new Sid(0), RegisterParams);
            _streams[1] = await Participant.OpenStreamAsync(new Sid(1), CharacterScreenParams);
            _streams[2] = await Participant.OpenStreamAsync(new Sid(2), InGameParams);
            _streams[3] = await Participant.OpenStreamAsync(new Sid(3), GeneralParams);
            _streams[4] = await Participant.OpenStreamAsync(new Sid(4), PingParams);
            _streams[5] = await Participant.OpenStreamAsync(new Sid(5), TerrainParams);
        }

        public void SetPosition(float3 pos) {
            Position = new Pos(pos);
        }

        public void SetOrientation(quaternion q) {
            Orientation = new Ori(q);
        }

        /// <summary>
        /// Send a pre-serialized message to the client using the configured stream.
        /// </summary>
        public async Task SendPreparedAsync(PreparedMsg msg) {
            if (!_streams.TryGetValue(msg.StreamId, out var stream))
                throw new InvalidOperationException($"Stream {msg.StreamId} not initialized");
            await stream.SendAsync(msg.Message);
        }

        public bool TryGetStream(byte id, out Stream stream) => _streams.TryGetValue(id, out stream);
    }
}
