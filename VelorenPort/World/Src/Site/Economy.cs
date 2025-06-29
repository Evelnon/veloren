using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site {
    /// <summary>Represents a simple trading route connecting multiple sites.</summary>
    [Serializable]
    public class TradingRoute {
        public List<Store<Site>.Id> Sites { get; } = new();
        public TradingRoute(IEnumerable<Store<Site>.Id> sites) { Sites.AddRange(sites); }
    }

    /// <summary>Population change event produced by the economy.</summary>
    public enum PopulationEventType { Birth, Death }

    [Serializable]
    public record PopulationEvent(PopulationEventType Type, Store<Npc>.Id NpcId, Store<Site>.Id SiteId);
    /// <summary>
    /// Entry point for a very small scale economy simulation.
    /// Each registered site advances its internal state every tick.
    /// </summary>
    public static class EconomySim {
        /// <summary>
        /// Progress the economy of all sites contained in <paramref name="index"/>.
        /// </summary>
        public static void SimulateEconomy(WorldIndex index, float dt) {
            foreach (var (_, site) in index.Sites.Enumerate()) {
                site.Economy.Tick(dt);
                // Produce a small amount of food each tick for demonstration purposes
                site.Economy.Produce(new Good.Food(), dt);
                site.Market.UpdatePrices(site.Economy);
            }
        }

        /// <summary>
        /// Advance all caravans and let them trade goods along their routes.
        /// </summary>
        public static void SimulateCaravans(WorldIndex index, System.Collections.Generic.IEnumerable<Caravan> caravans, float dt)
        {
            foreach (var car in caravans)
                car.Tick(index, dt);
        }

        /// <summary>
        /// Recalculate market prices for all sites based on current demand and stock.
        /// </summary>
        public static void UpdateMarkets(WorldIndex index)
        {
            foreach (var (_, site) in index.Sites.Enumerate())
                site.Market.UpdatePrices(site.Economy);
        }

        /// <summary>
        /// Transfer a small amount of coin between two sites. This is merely a
        /// placeholder for the complex trading logic of the original game.
        /// </summary>
        public static void TransferCoin(Site from, Site to, float amount) {
            if (from.Economy.Coin < amount || amount <= 0f) return;
            from.Economy.Coin -= amount;
            to.Economy.Coin += amount;
        }

        /// <summary>Deposit coins into a site's treasury.</summary>
        public static void Deposit(Site site, float amount) {
            if (amount > 0f) site.Economy.Coin += amount;
        }

        /// <summary>Withdraw coins from a site's treasury if possible.</summary>
        public static bool Withdraw(Site site, float amount) {
            if (amount <= 0f || site.Economy.Coin < amount) return false;
            site.Economy.Coin -= amount;
            return true;
        }

        /// <summary>Add a trading route to the world index.</summary>
        public static void AddTradingRoute(WorldIndex index, TradingRoute route)
            => index.TradingRoutes.Add(route);

        /// <summary>
        /// Simulate trading along predefined caravan routes.
        /// </summary>
        public static void SimulateTradingRoutes(WorldIndex index, float dt)
        {
            foreach (var route in index.CaravanRoutes)
            {
                if (route.Sites.Count < 2) continue;
                for (int i = 0; i < route.Sites.Count - 1; i++)
                {
                    var from = index.Sites[route.Sites[i]];
                    var to = index.Sites[route.Sites[i + 1]];
                    foreach (var (gidx, rate) in route.Goods.Iterate())
                    {
                        if (rate <= 0f) continue;
                        var good = gidx.ToGood();
                        float avail = from.Economy.GetStock(good);
                        float amount = MathF.Min(rate * dt, avail);
                        if (amount > 0f)
                            TradeGoods(from, to, good, amount);
                    }
                }
            }
        }

        /// <summary>Advance population counts and record events.</summary>
        public static void UpdatePopulation(WorldIndex index, float dt)
        {
            foreach (var (id, site) in index.Sites.Enumerate())
            {
                // Very small model: births occur if food stock exceeds population
                int population = site.Population.Count;
                float food = site.Economy.GetStock(new Good.Food());
                if (food > population)
                {
                    var uid = index.AllocateUid();
                    var npc = new Npc(uid) { Name = Site.NameGen.Generate(new Random((int)uid.Value)), Home = new SiteId(id.Value) };
                    var npcId = index.Npcs.Insert(npc);
                    site.Population.Add(npcId);
                    index.PopulationEvents.Add(new PopulationEvent(PopulationEventType.Birth, npcId, id));
                }
                else if (population > 0 && food < population * 0.25f)
                {
                    var npcId = site.Population[^1];
                    site.Population.RemoveAt(site.Population.Count - 1);
                    index.PopulationEvents.Add(new PopulationEvent(PopulationEventType.Death, npcId, id));
                }
            }
        }

        /// <summary>Collect a fixed tax from all sites and accumulate it in <paramref name="treasury"/>.</summary>
        public static void CollectTaxes(WorldIndex index, ref float treasury, float taxPerSite) {
            foreach (var (_, site) in index.Sites.Enumerate()) {
                if (site.Economy.Coin >= taxPerSite) {
                    site.Economy.Coin -= taxPerSite;
                    treasury += taxPerSite;
                }
            }
        }

        /// <summary>
        /// Transfer goods from one site to another if available.
        /// </summary>
        public static bool TradeGoods(Site from, Site to, Good good, float amount)
        {
            if (!from.Economy.Consume(good, amount))
                return false;
            to.Economy.Produce(good, amount);
            return true;
        }
    }
}
