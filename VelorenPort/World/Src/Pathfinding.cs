using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.World {
    /// <summary>
    /// Simple search configuration mirroring <c>SearchCfg</c> in the Rust code.
    /// </summary>
    [Serializable]
    public struct SearchCfg {
        public float PathDiscount;
        public float GradientAversion;

        /// <summary>
        /// Optional dynamic cost function that can be used to influence the
        /// search at runtime. This allows temporary navigation data such as
        /// obstacles or preferred tiles to affect the path result.
        /// </summary>
        public Func<int2, float>? DynamicCost;

        public SearchCfg(float pathDiscount, float gradientAversion,
                          Func<int2, float>? dynamicCost = null) {
            PathDiscount = pathDiscount;
            GradientAversion = gradientAversion;
            DynamicCost = dynamicCost;
        }
    }

    /// <summary>
    /// Stub pathfinder used by world generation.
    /// </summary>
    public struct Searcher {
        private readonly Land _land;
        public SearchCfg Cfg { get; }

        public Searcher(Land land, SearchCfg cfg) {
            _land = land;
            Cfg = cfg;
        }

        private static readonly int2[] DIRS =
        {
            new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1),
            new int2(1, 1), new int2(1, -1), new int2(-1, 1), new int2(-1, -1)
        };

        private IEnumerable<(int2 Node, float Cost)> Neighbors(int2 pos)
        {
            foreach (var dir in DIRS)
            {
                var next = pos + dir;
                float baseCost = math.length((float2)dir);
                int2 wpos = TerrainChunkSize.CposToWposCenter(next);
                float grad = _land.GetGradientApprox(wpos);
                float cost = baseCost * (1f + grad * Cfg.GradientAversion);
                if (Cfg.DynamicCost != null)
                    cost += Cfg.DynamicCost(next);
                yield return (next, cost);
            }
        }

        /// <summary>
        /// Attempt to find a path between two chunk positions using an A*
        /// search. Terrain gradient is taken into account via
        /// <see cref="SearchCfg.GradientAversion"/>.
        /// </summary>
        public Path<int2>? Search(int2 a, int2 b)
        {
            var astar = new AStar<int2>(100_000, a);
            var res = astar.Poll(
                100_000,
                p => EstimateCost(p, b),
                Neighbors,
                p => p.Equals(b));

            return res switch
            {
                PathResult<int2>.Complete complete => complete.Route,
                _ => null
            };
        }

        public static float EstimateCost(int2 a, int2 b) => math.length((float2)(a - b));
    }
}
