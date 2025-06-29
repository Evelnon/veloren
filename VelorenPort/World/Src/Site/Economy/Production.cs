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

    public void Produce(EconomyData economy, float dt)
    {
        foreach (var kv in Rates)
            economy.Produce(kv.Key, kv.Value * dt);
    }
}
