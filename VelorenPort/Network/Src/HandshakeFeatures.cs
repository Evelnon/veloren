using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Flags exchanged during the handshake to negotiate optional features.
    /// Mirrors the Rust bitflags.
    /// </summary>
    [Flags]
    public enum HandshakeFeatures : uint {
        None = 0,
        ReliableUdp = 1 << 0,
        Compression = 1 << 1,
        Encryption = 1 << 2,
    }
}
