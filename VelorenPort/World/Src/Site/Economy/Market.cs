using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.World.Util;

namespace VelorenPort.World.Site.Economy;

/// <summary>
/// Very small scale market tracking demand and prices for goods.
/// This is a simplified model compared to the Rust implementation.
/// </summary>
[Serializable]
public class Market
{
    public MapVec<Good, float> Prices { get; } = new(1f);
    public MapVec<Good, float> Demand { get; } = new(0f);

    /// <summary>Current price of a good or 1 if unknown.</summary>
    public float GetPrice(Good good) => Prices[good];

    /// <summary>Current demand for a good.</summary>
    public float GetDemand(Good good) => Demand[good];

    /// <summary>Add to the demand for the given good.</summary>
    public void AddDemand(Good good, float amount)
    {
        if (amount <= 0f) return;
        Demand[good] = GetDemand(good) + amount;
    }

    /// <summary>
    /// Update all prices based on demand and available stock.
    /// </summary>
    public void UpdatePrices(FullEconomy economy)
    {
        foreach (var (gidx, stock) in economy.Stocks.Iterate())
        {
            var good = gidx.ToGood();
            float demand = GetDemand(good);
            float basePrice = 1f;
            float price = basePrice * (1f + demand) / math.max(1f, stock);
            Prices[good] = price;
        }
    }
}
