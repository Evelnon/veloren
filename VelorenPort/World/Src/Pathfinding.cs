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
        /// Attempt to find a path between two chunks. Currently unimplemented
        /// like the Rust version.
        /// </summary>
        public Path<int2>? Search(int2 a, int2 b) {
            // TODO: implement this function
            return null;
        }
    }
}
