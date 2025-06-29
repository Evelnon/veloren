using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelorenPort.CoreEngine;
using VelorenPort.World.Civ;

namespace VelorenPort.World.Site.Economy;

/// <summary>
/// Tracks inter-site economy state similarly to the Rust <c>context.rs</c>.
/// Holds a log of goods movement and simple metrics per site.
/// </summary>
[Serializable]
public class EconomyContext
{
    [Serializable]
    public record Movement(Store<Site>.Id From, Store<Site>.Id To, Good Good, float Amount, float Time);

    [Serializable]
    public class Metrics
    {
        /// <summary>Total amount of goods exported from this site.</summary>
        public float Exported { get; set; }
        /// <summary>Total amount of goods imported into this site.</summary>
        public float Imported { get; set; }
    }

    /// <summary>Latest market price per site and good.</summary>
    public Dictionary<Store<Site>.Id, Dictionary<Good, float>> MarketPrices { get; } = new();

    [Serializable]
    public enum TradePhase { Plan, Execute, Collect }

    [Serializable]
    public record TradeEvent(TradePhase Phase, Store<Site>.Id From, Store<Site>.Id To, Good Good, float Amount, float Price, float Time);

    public float Time { get; private set; }
    public List<Movement> History { get; } = new();
    public Dictionary<Store<Site>.Id, Metrics> SiteMetrics { get; } = new();
    public List<TradeEvent> Events { get; } = new();
    public List<StageEvent> StageHistory { get; } = new();

    private void LogStage(EconomyStage stage)
        => StageHistory.Add(new StageEvent(stage, Time));

    /// <summary>Advance the economy simulation by <paramref name="dt"/> days.</summary>
    public void Tick(WorldIndex index, float dt)
    {
        LogStage(EconomyStage.Deliveries);
        EconomySim.SimulateCaravans(index, index.Caravans, dt);

        LogStage(EconomyStage.TickSites);
        EconomySim.SimulateEconomy(index, dt);

        LogStage(EconomyStage.DistributeOrders);
        EconomySim.SimulateTradingRoutes(index, dt);

        LogStage(EconomyStage.TradeAtSites);
        // trade executed within SimulateTradingRoutes for simplicity

        LogStage(EconomyStage.UpdateMarkets);
        EconomySim.UpdateMarkets(index);
        EconomySim.UpdatePopulation(index, dt);
        foreach (var (id, site) in index.Sites.Enumerate())
        {
            var dict = MarketPrices.GetValueOrDefault(id);
            if (dict == null)
            {
                dict = new Dictionary<Good, float>();
                MarketPrices[id] = dict;
            }
            foreach (var kv in site.Market.Prices)
                dict[kv.Key] = kv.Value;
        }
        Time += dt;
    }

    /// <summary>
    /// Perform a trade of <paramref name="amount"/> of <paramref name="good"/>
    /// from <paramref name="from"/> to <paramref name="to"/>.
    /// The movement is recorded and market prices updated.
    /// </summary>
    public bool Trade(WorldIndex index, Store<Site>.Id from, Store<Site>.Id to, Good good, float amount)
    {
        var siteFrom = index.Sites[from];
        var siteTo = index.Sites[to];
        if (!EconomySim.TradeGoods(siteFrom, siteTo, good, amount))
            return false;

        History.Add(new Movement(from, to, good, amount, Time));
        AddMetric(from, amount, exported: true);
        AddMetric(to, amount, exported: false);
        siteFrom.Market.UpdatePrices(siteFrom.Economy);
        siteTo.Market.UpdatePrices(siteTo.Economy);
        float price = siteFrom.Market.GetPrice(good);
        Events.Add(new TradeEvent(TradePhase.Execute, from, to, good, amount, price, Time));
        return true;
    }

    public void PlanTrade(WorldIndex index, Store<Site>.Id from, Store<Site>.Id to, Good good, float amount)
    {
        float price = index.Sites[from].Market.GetPrice(good);
        Events.Add(new TradeEvent(TradePhase.Plan, from, to, good, amount, price, Time));
    }

    public void CollectTrade(WorldIndex index, Store<Site>.Id from, Store<Site>.Id to, Good good, float amount)
    {
        float price = index.Sites[to].Market.GetPrice(good);
        Events.Add(new TradeEvent(TradePhase.Collect, from, to, good, amount, price, Time));
    }

    private void AddMetric(Store<Site>.Id id, float amount, bool exported)
    {
        if (!SiteMetrics.TryGetValue(id, out var m))
        {
            m = new Metrics();
            SiteMetrics[id] = m;
        }
        if (exported) m.Exported += amount; else m.Imported += amount;
    }

    /// <summary>Persist this context to <paramref name="path"/>.</summary>
    public void Save(string path)
    {
        var opts = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(path, JsonSerializer.Serialize(this, opts));
    }

    /// <summary>Load a context from <paramref name="path"/>.</summary>
    public static EconomyContext Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(path);
        var ctx = JsonSerializer.Deserialize<EconomyContext>(File.ReadAllText(path));
        return ctx ?? new EconomyContext();
    }
}
