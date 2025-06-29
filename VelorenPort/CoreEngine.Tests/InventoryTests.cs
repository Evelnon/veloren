using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;
using Xunit;

namespace CoreEngine.Tests;

public class InventoryTests
{
    [Fact]
    public void Add_IncrementsAmount()
    {
        var inv = new Inventory();
        var slot = new InvSlotId(0, 0);
        var item = new ItemDefinitionIdOwned.Simple("wood");
        inv.Add(slot, item, 1);
        inv.Add(slot, item, 2);
        Assert.Equal(3u, inv.GetAmount(slot));
    }

    [Fact]
    public void Remove_DecrementsAndDeletes()
    {
        var inv = new Inventory();
        var slot = new InvSlotId(0, 0);
        var item = new ItemDefinitionIdOwned.Simple("wood");
        inv.Add(slot, item, 5);
        inv.Remove(slot, 3);
        Assert.Equal(2u, inv.GetAmount(slot));
        inv.Remove(slot, 5);
        Assert.Equal(0u, inv.GetAmount(slot));
    }

    [Fact]
    public void SlotsWithId_ReportsEntries()
    {
        var inv = new Inventory();
        var slot = new InvSlotId(0, 1);
        var item = new ItemDefinitionIdOwned.Simple("stone");
        inv.Add(slot, item, 2);
        Assert.Contains(inv.SlotsWithId(), e => e.Slot.Equals(slot) && e.Amount == 2);
    }
}
