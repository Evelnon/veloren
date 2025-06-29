using System;

namespace VelorenPort.CoreEngine.comp {
    /// <summary>
    /// Very small approximation of the LootOwner component in Rust.
    /// Tracks which player owns dropped loot and when the ownership expires.
    /// </summary>
    [Serializable]
    public struct LootOwner {
        public Uid Owner;
        public bool Soft;
        public DateTime Expiry;
        private const int OwnershipSeconds = 45;

        public LootOwner(Uid owner, bool soft) {
            Owner = owner;
            Soft = soft;
            Expiry = DateTime.UtcNow.AddSeconds(OwnershipSeconds);
        }

        public bool Expired() => DateTime.UtcNow >= Expiry;
    }
}
