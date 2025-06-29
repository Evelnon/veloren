using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site;

/// <summary>
/// Simple caravan that moves goods along a fixed route of sites.
/// Each tick advances to the next site and transfers cargo.
/// This is a placeholder for the complex caravan system in the Rust code.
/// </summary>
[Serializable]
public class Caravan
{
    public List<Store<Site>.Id> Route { get; } = new();
    public int CurrentIndex { get; private set; }
    public float Progress { get; private set; }
    public float Speed { get; set; } = 1f;
    public Dictionary<Good, float> Cargo { get; } = new();

    public Caravan(IEnumerable<Store<Site>.Id> route)
    {
        Route.AddRange(route);
    }

    /// <summary>Site the caravan is currently travelling from.</summary>
    public Store<Site>.Id CurrentSite => Route[CurrentIndex];

    /// <summary>Advance the caravan along its route.</summary>
    public void Tick(WorldIndex index, float dt)
    {
        if (Route.Count < 2) return;
        Progress += dt * Speed;
        if (Progress < 1f) return;
        Progress = 0f;
        var fromId = Route[CurrentIndex];
        var toId = Route[(CurrentIndex + 1) % Route.Count];
        var from = index.Sites[fromId];
        var to = index.Sites[toId];

        // Load a small amount of food if available
        if (from.Economy.Consume(new Good.Food(), 1f))
            Load(new Good.Food(), 1f);

        // Deliver all cargo
        foreach (var kv in Cargo.ToList())
        {
            if (kv.Value <= 0f) continue;
            EconomySim.TradeGoods(from, to, kv.Key, kv.Value);
            Cargo[kv.Key] = 0f;
        }

        CurrentIndex = (CurrentIndex + 1) % Route.Count;
    }

    public void Load(Good good, float amount)
    {
        if (amount <= 0f) return;
        Cargo.TryGetValue(good, out var cur);
        Cargo[good] = cur + amount;
    }
}
