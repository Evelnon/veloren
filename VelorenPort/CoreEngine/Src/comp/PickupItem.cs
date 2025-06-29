using System;

namespace VelorenPort.CoreEngine.comp
{
    /// <summary>
    /// Simplified item stack used when spawning loot drops. Mirrors part of
    /// `common/src/comp/inventory/item/mod.rs`.
    /// </summary>
    [Serializable]
    public struct PickupItem
    {
        public string Name;
        public uint Amount;
        public DateTime Created;
        public DateTime NextMergeCheck;
        public bool ShouldMerge;

        private const double ChecksPerSecond = 10.0;

        public PickupItem(string name, uint amount, bool shouldMerge)
        {
            Name = name;
            Amount = amount;
            ShouldMerge = shouldMerge;
            Created = DateTime.UtcNow;
            NextMergeCheck = Created;
        }

        public bool CanMerge(PickupItem other)
            => ShouldMerge && other.ShouldMerge && Name == other.Name;

        public void Merge(ref PickupItem other)
        {
            if (!CanMerge(other)) return;
            Amount += other.Amount;
            other.Amount = 0;
        }

        /// <summary>Remove up to <paramref name="amount"/> from this stack.</summary>
        /// <returns>Actual amount removed.</returns>
        public uint Take(uint amount)
        {
            var taken = Math.Min(amount, Amount);
            Amount -= taken;
            return taken;
        }

        public void ScheduleNextMerge()
        {
            var age = (DateTime.UtcNow - Created).TotalSeconds;
            var delay = Math.Max(1.0 / ChecksPerSecond, age);
            NextMergeCheck = DateTime.UtcNow.AddSeconds(delay);
        }
    }
}
