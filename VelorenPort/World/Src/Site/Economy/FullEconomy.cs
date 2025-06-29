using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site.Economy;

/// <summary>
/// Extended economy data ported from the Rust implementation. This class
/// exposes population, stock tracking and neighbor information. Only a
/// lightweight subset of the original logic is implemented.
/// </summary>
[Serializable]
public class FullEconomy
{
    /// <summary>Approximate population count.</summary>
    public float Population { get; set; } = 32f;

    /// <summary>Per-good stock values.</summary>
    public GoodMap<float> Stocks { get; } = GoodMap<float>.FromDefault(0f);

    /// <summary>Current surplus of each good.</summary>
    public GoodMap<float> Surplus { get; } = GoodMap<float>.FromDefault(0f);

    /// <summary>Change rate of each good.</summary>
    public GoodMap<float> MarginalSurplus { get; } = GoodMap<float>.FromDefault(0f);

    /// <summary>Per-profession labor ratios.</summary>
    public LaborMap<float> Labors { get; } = LaborMap<float>.FromDefault(0f);

    /// <summary>Yield per labor per year.</summary>
    public LaborMap<float> Yields { get; } = LaborMap<float>.FromDefault(0f);

    /// <summary>Natural resources around the site.</summary>
    public NaturalResources Resources { get; } = new();

    /// <summary>Known neighbors for trade.</summary>
    public List<NeighborInformation> Neighbors { get; } = new();

    /// <summary>Outgoing trade orders.</summary>
    public Dictionary<Store<Site>.Id, List<TradeOrder>> Orders { get; } = new();

    /// <summary>Pending deliveries to this site.</summary>
    public Dictionary<Store<Site>.Id, List<TradeDelivery>> Deliveries { get; } = new();

    /// <summary>Update stock decay and replenish natural resources.</summary>
    public void Tick(float dt)
    {
        foreach (var (gidx, _))
            Stocks[gidx] = Math.Max(0f, Stocks[gidx] - Cache.Instance.DecayRate[gidx] * dt);
        Replenish(dt);
    }

    /// <summary>Add natural resources from a chunk.</summary>
    public void AddChunk(SimChunk chunk)
    {
        Resources.AddChunk(chunk);
    }

    /// <summary>Refill stocks based on long term resource averages.</summary>
    public void Replenish(float dt)
    {
        foreach (var (gidx, ch) in Resources.ChunksPerResource.Iterate())
        {
            float yield = Resources.AverageYieldPerChunk[gidx] * ch;
            Stocks[gidx] = Math.Max(Stocks[gidx], yield * dt);
        }
    }

    /// <summary>Plan trade with neighbors based on surplus and missing goods.</summary>
    public void PlanTradeForSite(Store<Site>.Id self)
    {
        foreach (var n in Neighbors)
        {
            var order = new TradeOrder(self, new GoodMap<float>());
            foreach (var (gidx, amount) in Surplus.Iterate())
            {
                if (amount < 0f)
                {
                    order.Amount[gidx] = -amount;
                }
            }
            if (!Orders.ContainsKey(n.Id))
                Orders[n.Id] = new List<TradeOrder>();
            Orders[n.Id].Add(order);
        }
    }

    /// <summary>Execute orders arriving from neighbors.</summary>
    public void TradeAtSite(Store<Site>.Id self)
    {
        if (!Deliveries.TryGetValue(self, out var list))
            return;
        foreach (var del in list)
        {
            foreach (var (gidx, amount) in del.Amount.Iterate())
            {
                Stocks[gidx] += amount;
            }
        }
        Deliveries.Remove(self);
    }
}

/// <summary>Information about a nearby site.</summary>
[Serializable]
public record NeighborInformation(Store<Site>.Id Id);

/// <summary>Outgoing trade request.</summary>
[Serializable]
public record TradeOrder(Store<Site>.Id Customer, GoodMap<float> Amount);

/// <summary>Incoming trade delivery.</summary>
[Serializable]
public record TradeDelivery(Store<Site>.Id Supplier, GoodMap<float> Amount);

/// <summary>Natural resource tracking around a site.</summary>
[Serializable]
public class NaturalResources
{
    public List<AreaResources> PerArea { get; } = new();
    public GoodMap<float> ChunksPerResource { get; } = GoodMap<float>.FromDefault(0f);
    public GoodMap<float> AverageYieldPerChunk { get; } = GoodMap<float>.FromDefault(0f);

    public void AddChunk(SimChunk chunk)
    {
        var area = new AreaResources();
        area.Chunks += 1;
        PerArea.Add(area);
    }
}

/// <summary>Aggregated resource data for a distance bucket.</summary>
[Serializable]
public class AreaResources
{
    public GoodMap<float> ResourceSum { get; } = GoodMap<float>.FromDefault(0f);
    public GoodMap<float> ResourceChunks { get; } = GoodMap<float>.FromDefault(0f);
    public uint Chunks { get; set; } = 0;
}

/// <summary>Map keyed by Labor enumeration.</summary>
[Serializable]
public class LaborMap<T> where T : struct
{
    private readonly Dictionary<Labor, T> _data = new();

    public T this[Labor labor]
    {
        get => _data.TryGetValue(labor, out var v) ? v : default;
        set => _data[labor] = value;
    }

    public static LaborMap<T> FromDefault(T value)
    {
        var map = new LaborMap<T>();
        foreach (Labor l in Enum.GetValues(typeof(Labor)))
            map._data[l] = value;
        return map;
    }
}

/// <summary>Subset of professions used by the economy.</summary>
[Serializable]
public enum Labor
{
    Farmer,
    Hunter,
    Blacksmith,
    Alchemist,
    Merchant,
    Guard,
    Everyone
}

