using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Information about a ban used in disconnect and register errors.
    /// Mirrors the BanInfo struct from the Rust networking code.
    /// </summary>
    [Serializable]
    public struct BanInfo {
        public string Reason { get; set; }
        public long? Until { get; set; }

        public BanInfo(string reason, long? until) {
            Reason = reason;
            Until = until;
        }
    }
}
