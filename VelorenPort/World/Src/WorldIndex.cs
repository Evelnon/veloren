using System;
using VelorenPort.CoreEngine;
using VelorenPort.World.Site;

namespace VelorenPort.World {
    /// <summary>
    /// Simplified index keeping track of global world state.
    /// </summary>
    [Serializable]
    public class WorldIndex {
        public uint Seed { get; private set; }
        public float Time { get; set; }
        public Noise Noise { get; private set; }
        public WorldMap Map { get; } = new WorldMap();
        public Store<Site.Site> Sites { get; } = new();

        public WorldIndex(uint seed) {
            Seed = seed;
            Noise = new Noise(seed);
        }
    }
}
