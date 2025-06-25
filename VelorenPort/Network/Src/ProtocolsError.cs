using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Aggregates lower level protocol errors. Mirrors the Rust enum of the same
    /// name where each variant carries context from a specific protocol.
    /// </summary>
    [Serializable]
    public abstract record ProtocolsError {
        public sealed record Tcp(Exception Error) : ProtocolsError;
        public sealed record Udp(Exception Error) : ProtocolsError;
        public sealed record Quic(QuicError Error) : ProtocolsError;
        public sealed record Mpsc(MpscError Error) : ProtocolsError;
    }
}
