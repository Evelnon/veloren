using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;
using Xunit;

namespace CoreEngine.Tests;

public class ReducedInventoryTests
{
    [Fact]
    public void From_CopiesAllSlots()
    {
        var inv = new Inventory();
        var slotA = new InvSlotId(0, 0);
        var slotB = new InvSlotId(1, 2);
        var itemA = new ItemDefinitionIdOwned.Simple("wood");
        var itemB = new ItemDefinitionIdOwned.Simple("stone");
        inv.Add(slotA, itemA, 3);
        inv.Add(slotB, itemB, 1);

        var reduced = ReducedInventory.From(inv);

        Assert.Equal(2, reduced.Inventory.Count);
        Assert.Equal(3u, reduced.Inventory[slotA].Amount);
        Assert.Equal(itemA, reduced.Inventory[slotA].Name);
        Assert.Equal(1u, reduced.Inventory[slotB].Amount);
        Assert.Equal(itemB, reduced.Inventory[slotB].Name);
    }
}
