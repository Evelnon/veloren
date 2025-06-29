using System;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site {
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
