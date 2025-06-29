using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelorenPort.CoreEngine;

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

    public float Time { get; private set; }
    public List<Movement> History { get; } = new();
    public Dictionary<Store<Site>.Id, Metrics> SiteMetrics { get; } = new();

    /// <summary>Advance the economy simulation by <paramref name="dt"/> days.</summary>
    public void Tick(WorldIndex index, float dt)
    {
        EconomySim.SimulateEconomy(index, dt);
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
        return true;
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
