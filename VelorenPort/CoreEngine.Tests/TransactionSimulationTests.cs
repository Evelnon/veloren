using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.Store;
using Xunit;

namespace CoreEngine.Tests;

public class TransactionSimulationTests
{
    [Fact]
    public void Price_Increases_When_Supply_Drops()
    {
        var store = new MerchantStore(seed: 1) { RandomFluctuationRange = 0f };
        var item = new ItemDefinitionIdOwned.Simple("wood");
        store.AddItem(item, 5, 10f);
        Assert.True(store.TryGetPrice(item, 1, out var initial));
        Assert.True(store.Buy(item, 1));
        Assert.True(store.TryGetPrice(item, 1, out var after));
        Assert.True(after > initial);
    }

    [Fact]
    public void Different_Players_Have_Different_Prices()
    {
        var store = new MerchantStore(seed: 1) { RandomFluctuationRange = 0f };
        var item = new ItemDefinitionIdOwned.Simple("stone");
        store.AddItem(item, 5, 10f);
        var a = new Uid(1);
        var b = new Uid(2);
        store.AdjustReputation(a, 0.5f);
        store.AdjustReputation(b, -0.5f);
        Assert.True(store.TryGetPrice(item, 1, a, out var cheap));
        Assert.True(store.TryGetPrice(item, 1, b, out var costly));
        Assert.True(cheap < costly);
    }
}
