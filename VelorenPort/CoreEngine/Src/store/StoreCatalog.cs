using System;
using System.Collections.Generic;
using System.Linq;

namespace VelorenPort.CoreEngine.Store;

/// <summary>
/// Dynamic mapping of store items to their available quantities and prices.
/// </summary>
[Serializable]
public class StoreCatalog
{
    [Serializable]
    public record StoreEntry(uint Amount, float BasePrice, float Demand = 0f);

    public Dictionary<ItemDefinitionIdOwned, StoreEntry> Items { get; } = new();

    public void Add(ItemDefinitionIdOwned item, uint amount, float basePrice)
        => Items[item] = new StoreEntry(amount, basePrice);

    public bool TryGet(ItemDefinitionIdOwned item, out (uint Amount, float BasePrice, float Demand) data)
    {
        if (Items.TryGetValue(item, out var entry))
        {
            data = (entry.Amount, entry.BasePrice, entry.Demand);
            return true;
        }
        data = default;
        return false;
    }

    public IEnumerable<(ItemDefinitionIdOwned Item, uint Amount, float BasePrice, float Demand)> Entries()
        => Items.Select(kv => (kv.Key, kv.Value.Amount, kv.Value.BasePrice, kv.Value.Demand));
}
