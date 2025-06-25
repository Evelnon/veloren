using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Errors produced by the QUIC implementation. This mirrors the
    /// <c>QuicError</c> enum in the Rust networking layer.
    /// </summary>
    [Serializable]
    public abstract record QuicError {
        public sealed record Send(Exception Error) : QuicError;
        public sealed record Connection(Exception Error) : QuicError;
        public sealed record Write(Exception Error) : QuicError;
        public sealed record Read(Exception Error) : QuicError;
        public sealed record InternalMpsc : QuicError;
    }
}
