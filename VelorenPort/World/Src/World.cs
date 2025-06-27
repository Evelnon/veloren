using System;
using Unity.Mathematics;
using VelorenPort.World.Site;

namespace VelorenPort.World {
    /// <summary>
    /// Entry point of the world module. Provides a subset of the original
    /// functionality of <c>world/src/lib.rs</c>.
    /// </summary>
    [Serializable]
    public class World {
        public WorldSim Sim { get; }
        public WorldIndex Index { get; }

        private World(WorldSim sim, WorldIndex index) {
            Sim = sim;
            Index = index;
        }

        /// <summary>Generate a new world using the given seed.</summary>
        public static (World world, WorldIndex index) Generate(uint seed) {
            var index = new WorldIndex(seed);
            var sim = new WorldSim(seed, new int2(256, 256));
            var world = new World(sim, index);
            return (world, index);
        }

        public Land GetLand() => Land.FromSim(Sim);

        /// <summary>
        /// Sample a world column at the given world position using the internal simulation.
        /// </summary>
        public ColumnSample? SampleColumn(int2 wpos) {
            var land = GetLand();
            return land.ColumnSample(wpos, Index);
        }

        /// <summary>Advance the simulation by the specified delta time.</summary>
        public void Tick(float dt) {
            Economy.SimulateEconomy(Index, dt);
        }

        public Site.Site CreateSite(int2 position) {
            var site = new Site.Site { Position = position };
            Index.Sites.Insert(site);
            return site;
        }
    }
}
