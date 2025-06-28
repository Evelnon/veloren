using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.World.Site;
using VelorenPort.CoreEngine;

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

        public Noise Noise => Index.Noise;

        /// <summary>Advance the simulation by the specified delta time.</summary>
        public void Tick(float dt) {
            EconomySim.SimulateEconomy(Index, dt);
            Sim.Tick(dt);
        }

        /// <summary>
        /// Fetch metadata about neighbouring regions around <paramref name="chunkPos"/>.
        /// </summary>
        public IEnumerable<RegionInfo> NearbyRegions(int2 chunkPos, int radius)
            => Sim.GetNearRegions(chunkPos, radius);

        /// <summary>Get a map of altitudes around a chunk position.</summary>
        public float[,] GetAltitudeMap(int2 cpos, int radius) => Sim.GetAltitudeMap(cpos, radius);

        public Site.Site CreateSite(int2 position) {
            var rng = new Random((int)math.hash(position));
            string name = Site.NameGen.Generate(rng);
            var site = new Site.Site { Position = position, Name = name };
            Index.Sites.Insert(site);
            return site;
        }

        public void RemoveSite(Store<Site.Site>.Id id) => Index.Sites.Remove(id);

        /// <summary>
        /// Find a walkable position near <paramref name="wpos"/>.
        /// The search scans vertically within the chunk to locate a space with
        /// solid ground and enough headroom for an entity. This mirrors the
        /// behaviour of the Rust implementation in a simplified form.
        /// </summary>
        public float3 FindAccessiblePos(int2 wpos, bool ascending)
        {
            int2 cpos = TerrainChunkSize.WposToCpos(wpos);
            var chunk = Index.Map.GetOrGenerate(cpos, Noise);
            int2 local = wpos - cpos * Chunk.Size;

            int z = ascending ? 0 : Chunk.Height - 1;
            int step = ascending ? 1 : -1;
            while (z >= 0 && z < Chunk.Height - 2)
            {
                bool belowSolid = z > 0 && chunk[local.x, local.y, z - 1].IsFilled;
                bool space = !chunk[local.x, local.y, z].IsFilled &&
                             !chunk[local.x, local.y, z + 1].IsFilled &&
                             !chunk[local.x, local.y, z + 2].IsFilled;
                if (belowSolid && space)
                    return new float3(wpos.x + 0.5f, wpos.y + 0.5f, z + 0.5f);
                z += step;
            }

            return new float3(wpos.x + 0.5f, wpos.y + 0.5f, math.clamp(z, 0, Chunk.Height - 1) + 0.5f);
        }
    }
}
