using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site.Economy;

/// <summary>
/// Very small scale market tracking demand and prices for goods.
/// This is a simplified model compared to the Rust implementation.
/// </summary>
[Serializable]
public class Market
{
    public Dictionary<Good, float> Prices { get; } = new();
    public Dictionary<Good, float> Demand { get; } = new();

    /// <summary>Current price of a good or 1 if unknown.</summary>
    public float GetPrice(Good good) => Prices.TryGetValue(good, out var p) ? p : 1f;

    /// <summary>Current demand for a good.</summary>
    public float GetDemand(Good good) => Demand.TryGetValue(good, out var d) ? d : 0f;

    /// <summary>Add to the demand for the given good.</summary>
    public void AddDemand(Good good, float amount)
    {
        if (amount <= 0f) return;
        Demand[good] = GetDemand(good) + amount;
    }

    /// <summary>
    /// Update all prices based on demand and available stock.
    /// </summary>
    public void UpdatePrices(EconomyData economy)
    {
        foreach (var kv in economy.Stocks)
        {
            Good good = kv.Key;
            float stock = kv.Value;
            float demand = GetDemand(good);
            float basePrice = 1f;
            float price = basePrice * (1f + demand) / math.max(1f, stock);
            Prices[good] = price;
        }
    }
}
