using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents errors that can occur during protocol operations.
    /// Mirrors the generic <c>ProtocolError</c> enum in the Rust code.
    /// </summary>
    [Serializable]
    public abstract record ProtocolError<T> {
        public sealed record Custom(T Error) : ProtocolError<T>;
        public sealed record Violated : ProtocolError<T>;
    }
}
