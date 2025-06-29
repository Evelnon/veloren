using System.Collections.Generic;

namespace VelorenPort.CoreEngine.Store;

/// <summary>
/// Provides dynamic pricing information for <see cref="Good"/> categories.
/// </summary>
public class TradeCatalog
{
    private readonly Dictionary<Good, float> _values = new();

    public void SetPrice(Good good, float price) => _values[good] = price;

    public bool TryGetPrice(Good good, out float price) => _values.TryGetValue(good, out price);
}
