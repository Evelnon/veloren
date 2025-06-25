using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Errors that can occur during the protocol handshake. This generic
    /// structure mirrors the Rust <c>InitProtocolError&lt;E&gt;</c> enum.
    /// </summary>
    [Serializable]
    public abstract record InitProtocolError<T> {
        public sealed record Custom(T Error) : InitProtocolError<T>;
        public sealed record NotHandshake : InitProtocolError<T>;
        public sealed record NotId : InitProtocolError<T>;
        public sealed record WrongMagicNumber(byte[] Number) : InitProtocolError<T>;
        public sealed record WrongVersion(uint[] Version) : InitProtocolError<T>;
    }
}
