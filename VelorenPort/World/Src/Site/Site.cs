using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site {
    /// <summary>
    /// Basic site representation with a minimal economy model.
    /// </summary>
    [Serializable]
    public class Site {
        public int2 Position { get; init; }
        public string Name { get; set; } = "Site";
        public EconomyData Economy { get; } = new EconomyData();
    }

    /// <summary>
    /// Simplified economy data attached to each site.
    /// </summary>
    [Serializable]
    public class EconomyData {
        public Dictionary<Good, float> Stocks { get; } = new();
        public float Coin { get; set; } = 0f;

        /// <summary>Retrieve the amount of <paramref name="good"/> in stock.</summary>
        public float GetStock(Good good)
            => Stocks.TryGetValue(good, out var v) ? v : 0f;

        /// <summary>Increase the stock of <paramref name="good"/> by <paramref name="amount"/>.</summary>
        public void Produce(Good good, float amount)
        {
            if (amount <= 0f) return;
            Stocks[good] = GetStock(good) + amount;
        }

        /// <summary>
        /// Attempt to remove <paramref name="amount"/> of <paramref name="good"/> from the stock.
        /// </summary>
        /// <returns><c>true</c> if the stock contained at least that amount.</returns>
        public bool Consume(Good good, float amount)
        {
            if (amount <= 0f) return true;
            var current = GetStock(good);
            if (current < amount) return false;
            if (current == amount) Stocks.Remove(good);
            else Stocks[good] = current - amount;
            return true;
        }

        /// <summary>
        /// Advance the economy simulation by <paramref name="dt"/> days.
        /// </summary>
        public void Tick(float dt) {
            Coin += dt;
        }
    }
}
