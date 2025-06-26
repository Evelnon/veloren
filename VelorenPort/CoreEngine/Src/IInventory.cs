using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    public interface IInventory {
        uint GetAmount(InvSlotId slot);
        IEnumerable<(InvSlotId Slot, ItemDefinitionIdOwned Item, uint Amount)> SlotsWithId();
    }
}
