using VelorenPort.CoreEngine.Store;

namespace CoreEngine.Tests;

public class StoreCatalogTests
{
    [Fact]
    public void AddAndRetrieveItem()
    {
        var catalog = new StoreCatalog();
        var item = new ItemDefinitionIdOwned.Simple("wood");
        catalog.Add(item, 5, 2.5f);
        Assert.True(catalog.TryGet(item, out var data));
        Assert.Equal((5u, 2.5f), data);
    }
}
