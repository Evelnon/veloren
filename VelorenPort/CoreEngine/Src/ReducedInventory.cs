using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public record ReducedInventoryItem(ItemDefinitionIdOwned Name, uint Amount);

    [Serializable]
    public class ReducedInventory {
        public Dictionary<InvSlotId, ReducedInventoryItem> Inventory { get; } = new();

        public static ReducedInventory From(IInventory inventory) {
            var result = new ReducedInventory();
            foreach (var entry in inventory.SlotsWithId()) {
                result.Inventory[entry.Slot] = new ReducedInventoryItem(entry.Item, entry.Amount);
            }
            return result;
        }
    }
}
