using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.World.Site.Economy;

using VelorenPort.World.Site.Tile;
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


        public FullEconomy Economy { get; } = new FullEconomy();
        public Economy.Market Market { get; } = new Economy.Market();
        public Economy.Production Production { get; } = new Economy.Production();
        public List<PointOfInterest> PointsOfInterest { get; } = new();
        public List<Plot> Plots { get; } = new();
        public TileGrid Tiles { get; } = new TileGrid();
        public List<Store<Npc>.Id> Population { get; } = new();

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

}
