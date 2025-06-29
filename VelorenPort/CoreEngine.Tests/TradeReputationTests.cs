using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class TradeReputationTests
{
    [Fact]
    public void ApplyReputation_ModifiesPrice()
    {
        float basePrice = 10f;
        var cheaper = TradeUtils.ApplyReputation(basePrice, 1f);
        var expensive = TradeUtils.ApplyReputation(basePrice, -1f);
        Assert.True(cheaper < basePrice);
        Assert.True(expensive > basePrice);
    }

    [Fact]
    public void SitePrices_Balance_WithReputation()
    {
        var prices = new SitePrices();
        prices.AddPrice(new Good.Wood(), 5f);

        var inv = new ReducedInventory();
        var slot = new InvSlotId(0, 0);
        inv.Inventory[slot] = new ReducedInventoryItem(new ItemDefinitionIdOwned.Simple("log"), 1);
        TradePricing.Instance.Register(new ItemDefinitionIdOwned.Simple("log"), new[] { (1f, (Good)new Good.Wood()) });

        var offers = new[] { new Dictionary<InvSlotId, uint>(), new Dictionary<InvSlotId, uint>() };
        offers[0][slot] = 1;
        var inventories = new ReducedInventory?[] { inv, inv };
        var neutral = prices.Balance(offers, inventories, 0, reduce: false, reputation: 0f)!.Value;
        var positive = prices.Balance(offers, inventories, 0, reduce: false, reputation: 1f)!.Value;
        Assert.True(positive < neutral);
    }
}
