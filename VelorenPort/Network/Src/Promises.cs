using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Flags that modify Stream behaviour.
    /// Mirrors the Promises bitflags from Rust.
    /// </summary>
    [Flags]
    public enum Promises : byte {
        Ordered = 1 << 0,
        Consistency = 1 << 1,
        GuaranteedDelivery = 1 << 2,
        Compressed = 1 << 3,
        Encrypted = 1 << 4,
    }
}
