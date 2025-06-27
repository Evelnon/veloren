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

        /// <summary>
        /// Advance the economy simulation by <paramref name="dt"/> days.
        /// </summary>
        public void Tick(float dt) {
            Coin += dt;
        }
    }
}
