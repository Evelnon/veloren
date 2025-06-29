using System;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Identifier for the QUIC stream a frame belongs to. Replicates the Rust <c>QuicDataFormatStream</c> enum.
    /// </summary>
    public abstract record QuicDataFormatStream {
        public sealed record Main : QuicDataFormatStream;
        public sealed record Reliable(Sid Sid) : QuicDataFormatStream;
        public sealed record Unreliable : QuicDataFormatStream;
    }

    /// <summary>
    /// Data container used by the QUIC drain/sink implementations.
    /// </summary>
    public record QuicDataFormat(QuicDataFormatStream Stream, byte[] Data);
}
