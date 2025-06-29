using System.IO;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.Store;

namespace CoreEngine.Tests;

public class MerchantStoreTests
{
    [Fact]
    public void SaveAndLoadInventory()
    {
        var store = new MerchantStore();
        var item = new ItemDefinitionIdOwned.Simple("wood");
        store.AddItem(item, 3, 2f);
        string path = Path.GetTempFileName();
        store.Save(path);

        var loaded = new MerchantStore();
        loaded.Load(path);
        File.Delete(path);

        Assert.True(loaded.Catalog.TryGet(item, out var data));
        Assert.Equal((3u, 2f, 0f), data);
    }

    [Fact]
    public void Buy_RespectsReputation()
    {
        var store = new MerchantStore(seed: 0) { RandomFluctuationRange = 0f };
        var item = new ItemDefinitionIdOwned.Simple("wood");
        store.AddItem(item, 5, 10f);
        store.AdjustReputation(new Uid(1), 0.5f); // 5% discount

        Assert.True(store.TryGetPrice(item, 2, new Uid(1), out var price));
        Assert.InRange(price, 18f, 18.1f);
        Assert.True(store.Buy(item, 2, new Uid(1)));
        Assert.True(store.Catalog.TryGet(item, out var data2));
        Assert.Equal(3u, data2.Amount);
    }
}
