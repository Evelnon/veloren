using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Reason a server disconnected a client. Matches the enum in
    /// common/net/msg/server.rs but expressed as a discriminated union.
    /// </summary>
    [Serializable]
    public abstract record ServerDisconnectReason {
        public sealed record Shutdown : ServerDisconnectReason;
        public sealed record Kicked(string Reason) : ServerDisconnectReason;
        public sealed record Banned(BanInfo Info) : ServerDisconnectReason;
    }
}
