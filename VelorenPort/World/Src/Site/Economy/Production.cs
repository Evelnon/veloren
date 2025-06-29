using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site.Economy;

/// <summary>
/// Simple production schedule tracking output of goods per day.
/// </summary>
[Serializable]
public class Production
{
    public Dictionary<Good, float> Rates { get; } = new();

    public void SetRate(Good good, float rate) => Rates[good] = rate;

    public void Produce(FullEconomy economy, float dt)
    {
        foreach (var kv in Rates)
        {
            if (!GoodIndex.TryFromGood(kv.Key, out var gi))
                continue;
            if (kv.Value * dt <= 0f) continue;
            economy.Stocks[gi] += kv.Value * dt;
        }
    }
}
