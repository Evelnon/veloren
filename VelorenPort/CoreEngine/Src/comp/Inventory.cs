using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine.comp
{
    /// <summary>
    /// Simplified inventory container used for testing and basic trading.
    /// Provides item stacks indexed by <see cref="InvSlotId"/>.
    /// </summary>
    [Serializable]
    public class Inventory : IInventory
    {
        private readonly Dictionary<InvSlotId, (ItemDefinitionIdOwned Item, uint Amount)> _slots = new();

        /// <summary>Add an item stack to the given slot, stacking if possible.</summary>
        public void Add(InvSlotId slot, ItemDefinitionIdOwned item, uint amount)
        {
            if (_slots.TryGetValue(slot, out var entry))
            {
                if (Equals(entry.Item, item))
                    _slots[slot] = (item, entry.Amount + amount);
                else
                    _slots[slot] = (item, amount);
            }
            else
            {
                _slots[slot] = (item, amount);
            }
        }

        /// <summary>Remove items from a slot, deleting it when empty.</summary>
        public void Remove(InvSlotId slot, uint amount)
        {
            if (!_slots.TryGetValue(slot, out var entry)) return;
            if (amount >= entry.Amount)
                _slots.Remove(slot);
            else
                _slots[slot] = (entry.Item, entry.Amount - amount);
        }

        public uint GetAmount(InvSlotId slot) => _slots.TryGetValue(slot, out var entry) ? entry.Amount : 0u;

        public IEnumerable<(InvSlotId Slot, ItemDefinitionIdOwned Item, uint Amount)> SlotsWithId()
        {
            foreach (var (slot, entry) in _slots)
                yield return (slot, entry.Item, entry.Amount);
        }
    }
}
