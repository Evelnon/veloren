using System;
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

        public SearchCfg(float pathDiscount, float gradientAversion) {
            PathDiscount = pathDiscount;
            GradientAversion = gradientAversion;
        }
    }

    /// <summary>
    /// Stub pathfinder used by world generation.
    /// </summary>
    public struct Searcher {
        private readonly object _land; // placeholder for WorldSim
        public SearchCfg Cfg { get; }

        public Searcher(object land, SearchCfg cfg) {
            _land = land;
            Cfg = cfg;
        }

        /// <summary>
        /// Attempt to find a path between two chunk positions.
        /// This is a naive straight-line search that ignores terrain and simply
        /// walks one tile at a time towards the goal. It is a placeholder until
        /// the full Rust logic is ported.
        /// </summary>
        public Path<int2>? Search(int2 a, int2 b) {
            var path = new Path<int2>();
            var current = a;
            path.Add(current);
            int safety = 0;
            while (!current.Equals(b)) {
                int2 step = int2.zero;
                if (current.x < b.x) step.x = 1;
                else if (current.x > b.x) step.x = -1;
                if (current.y < b.y) step.y = 1;
                else if (current.y > b.y) step.y = -1;
                current += step;
                path.Add(current);
                if (++safety > 4096) return null; // avoid infinite loops
            }
            return path;
        }

        public static float EstimateCost(int2 a, int2 b) => math.length((float2)(a - b));
    }
}
