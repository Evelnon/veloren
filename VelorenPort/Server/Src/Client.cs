
using System;
using Unity.Mathematics;
using VelorenPort.Network;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server {
    /// <summary>
    /// Represents a connected game client with its participant handle.
    /// Additional per-client state will be added as systems are ported.
    /// </summary>
    public class Client {
        public Participant Participant { get; }
        public Pos Position { get; private set; }
        public Presence Presence { get; }
        public RegionSubscription RegionSubscription { get; }

        internal Client(Participant participant) {
            Participant = participant;
            Position = new Pos(float3.zero);
            Presence = new Presence(new ViewDistances(8, 8), new PresenceKind.Spectator());
            RegionSubscription = RegionUtils.InitializeRegionSubscription(Position, Presence);
        }

        public void SetPosition(float3 pos) {
            Position = new Pos(pos);
        }
    }
}
