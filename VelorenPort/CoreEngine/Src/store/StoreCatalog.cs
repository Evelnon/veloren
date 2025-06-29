using System.Collections.Generic;

namespace VelorenPort.CoreEngine.Store;

/// <summary>
/// Dynamic mapping of store items to their available quantities and prices.
/// </summary>
public class StoreCatalog
{
    private readonly Dictionary<ItemDefinitionIdOwned, (uint Amount, float Price)> _items = new();

    public void Add(ItemDefinitionIdOwned item, uint amount, float price)
        => _items[item] = (amount, price);

    public bool TryGet(ItemDefinitionIdOwned item, out (uint Amount, float Price) data)
        => _items.TryGetValue(item, out data);
}
