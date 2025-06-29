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
        public int2 Origin { get; set; }
        public string Name { get; set; } = "Site";

        /// <summary>
        /// Optional categorisation of the site. Mirrors the Rust <c>SiteKind</c>
        /// enum. Most generators currently set this to <c>SiteKind.Refactor</c>.
        /// </summary>
        public SiteKind Kind { get; set; } = SiteKind.Refactor;


        public EconomyData Economy { get; } = new EconomyData();
        public List<PointOfInterest> PointsOfInterest { get; } = new();
        public List<Plot> Plots { get; } = new();
        public TileGrid Tiles { get; } = new TileGrid();

        /// <summary>
        /// Retrieve spawn rules affecting the surroundings of this site. The
        /// current implementation returns default values only, awaiting a full
        /// port of the original logic.
        /// </summary>
        public SpawnRules SpawnRules(int2 wpos) => SpawnRules.Default;

        /// <summary>
        /// Metadata describing this site's category.
        /// </summary>
        public SiteKindMeta? Meta() => Kind.Meta();

        /// <summary>
        /// Whether this site should run the economy simulation.
        /// </summary>
        public bool DoEconomicSimulation() => Kind.ShouldDoEconomicSimulation();

        /// <summary>
        /// Bounding box of the site based on plot positions.
        /// </summary>
        public Aabr Bounds
        {
            get
            {
                if (Plots.Count == 0)
                    return new Aabr(Position - new int2(5, 5), Position + new int2(5, 5));
                int2 min = Position;
                int2 max = Position;
                foreach (var plot in Plots)
                {
                    int2 abs = Position + plot.LocalPos;
                    min = math.min(min, abs);
                    max = math.max(max, abs + 1);
                }
                min -= new int2(5, 5);
                max += new int2(5, 5);
                return new Aabr(min, max);
            }
        }

        /// <summary>Approximate radius of the site for collision checks.</summary>
        public float Radius()
        {
            int2 size = Bounds.Max - Bounds.Min;
            return math.length((float2)size) * 0.5f;
        }
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
