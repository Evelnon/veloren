
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using VelorenPort.Network;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;

namespace VelorenPort.Server {
    /// <summary>
    /// Represents a connected game client with its participant handle.
    /// Additional per-client state will be added as systems are ported.
    /// </summary>
    public class Client {
        public Participant Participant { get; }
        public Uid Uid { get; }
        public Pos Position { get; private set; }
        public Presence Presence { get; }
        public RegionSubscription RegionSubscription { get; }
        public ConnectAddr ConnectedFromAddr { get; }
        private readonly Dictionary<byte, Stream> _streams = new();
        public HashSet<int2> LoadedChunks { get; } = new();
        public PendingInvites PendingInvites { get; } = new();

        internal Client(Participant participant) {
            Participant = participant;
            Uid = new Uid((ulong)participant.Id.Value);
            ConnectedFromAddr = participant.ConnectedFrom;
            Position = new Pos(float3.zero);
            Presence = new Presence(new ViewDistances(8, 8), new PresenceKind.Spectator());
            RegionSubscription = RegionUtils.InitializeRegionSubscription(Position, Presence);
        }

        public void SetPosition(float3 pos) {
            Position = new Pos(pos);
        }

        /// <summary>
        /// Send a pre-serialized message to the client. Streams are lazily
        /// created based on the <see cref="PreparedMsg.StreamId"/>.
        /// </summary>
        public async Task SendPreparedAsync(PreparedMsg msg) {
            if (!_streams.TryGetValue(msg.StreamId, out var stream)) {
                stream = await Participant.OpenStreamAsync(
                    new Sid(msg.StreamId),
                    new StreamParams(Promises.Ordered | Promises.GuaranteedDelivery));
                _streams[msg.StreamId] = stream;
            }
            await stream.SendAsync(msg.Message);
        }
    }
}
