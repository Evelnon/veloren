using System;
using System.Collections.Generic;
using System.Linq;
using VelorenPort.NativeMath;
using VelorenPort.World.Site;
using VelorenPort.CoreEngine;
using VelorenPort.World.Layer;
using VelorenPort.World.Civ;

namespace VelorenPort.World
{
    /// <summary>
    /// Entry point of the world module. Provides a subset of the original
    /// functionality of <c>world/src/lib.rs</c>.
    /// </summary>
    [Serializable]
    public class World
    {
        public WorldSim Sim { get; }
        public WorldIndex Index { get; }

        private World(WorldSim sim, WorldIndex index)
        {
            Sim = sim;
            Index = index;
        }

        /// <summary>Generate a new world using the given seed.</summary>
        public static (World world, WorldIndex index) Generate(uint seed)
        {
            var index = new WorldIndex(seed);
            var sim = new WorldSim(seed, new int2(256, 256));
            var world = new World(sim, index);
            return (world, index);
        }

        /// <summary>
        /// Create an empty world instance with no chunks or sites.
        /// </summary>
        public static (World world, WorldIndex index) Empty()
        {
            var index = new WorldIndex(0);
            var sim = WorldSim.Empty();
            var world = new World(sim, index);
            return (world, index);
        }

        public Land GetLand() => Land.FromSim(Sim);

        /// <summary>
        /// Provide access to column sampling utilities.
        /// </summary>
        public ColumnGen SampleColumns() => new ColumnGen(Sim);

        /// <summary>
        /// Provide access to simple block sampling utilities.
        /// </summary>
        public BlockGen SampleBlocks() => new BlockGen(new ColumnGen(Sim));

        /// <summary>
        /// Generate a terrain chunk at <paramref name="chunkPos"/>.
        /// </summary>
        public (Chunk chunk, ChunkSupplement supplement) GenerateChunk(int2 chunkPos)
        {

            var chunk = TerrainGenerator.GenerateChunk(chunkPos, Noise);
            var supplement = new ChunkSupplement();
            var ctx = new Layer.LayerContext
            {
                ChunkPos = chunkPos,
                Rng = new Random((int)math.hash(chunkPos)),
                Supplement = supplement,
                ScatterChance = 0.1
            };
            Layer.LayerManager.Apply(Layer.LayerType.Spot, ctx);
            return (chunk, supplement);
        }

        /// <summary>
        /// Retrieve a lightweight world map snapshot with site and POI data.
        /// </summary>
        public WorldMapMsg GetMapData()
        {
            var msg = new WorldMapMsg
            {
                Dimensions = Sim.GetSize(),
                MaxHeight = Chunk.Height
            };

            var sitesWithId = new List<(Store<Site.Site>.Id id, Site.Site site)>();
            foreach (var (id, site) in Index.Sites.Enumerate())
            {
                var kind = SiteKindExtensions.Marker(site.Kind) ?? MarkerKind.Unknown;
                msg.Sites.Add(new Marker { Name = site.Name, Position = site.Position, Kind = kind });
                foreach (var poi in site.PointsOfInterest)
                    msg.Pois.Add(new PoiInfo { Name = poi.Description, Position = poi.Position, Kind = poi.Kind });

            }

            // Determine candidate starting sites based on simple heuristics
            int2 mapBlocks = TerrainChunkSize.Blocks(Sim.GetSize());
            float2 center = (float2)mapBlocks / 2f;
            float maxDist = math.length((float2)mapBlocks) / 2f;

            var scored = new List<(Store<Site.Site>.Id id, float score)>();
            foreach (var (id, site) in sitesWithId)
            {
                float baseScore = site.Kind == SiteKind.Refactor ? 2f :
                    (site.Meta() is SiteKindMeta.Settlement ? 1f : 0f);
                if (baseScore == 0f) { scored.Add((id, 0f)); continue; }
                float dist = math.length((float2)site.Position - center);
                float posScore = 1f - math.clamp(dist / maxDist, 0f, 1f);
                scored.Add((id, baseScore * posScore));

            }

            foreach (var entry in scored.OrderByDescending(s => s.score).Take(5))
                msg.PossibleStartingSites.Add(entry.id.Value);

            return msg;
        }

        /// <summary>
        /// Sample a world column at the given world position using the internal simulation.
        /// </summary>
        public ColumnSample? SampleColumn(int2 wpos)
        {
            var land = GetLand();
            return land.ColumnSample(wpos, Index);
        }

        public Noise Noise => Index.Noise;

        /// <summary>Advance the simulation by the specified delta time.</summary>
        public void Tick(float dt)
        {
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

        public Site.Site CreateSite(int2 position)
        {
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

        /// <summary>
        /// Build a level-of-detail zone containing simplified objects such as
        /// trees and structures. This is a very small subset of the original
        /// Rust implementation but allows the server to display distant terrain.
        /// </summary>
        public Zone GetLodZone(int2 zonePos)
        {
            int2 wmin = new int2(
                zonePos.x * (int)Lod.ZoneSize * TerrainChunkSize.RectSize.x,
                zonePos.y * (int)Lod.ZoneSize * TerrainChunkSize.RectSize.y);
            int2 wmax = wmin + new int2(
                (int)Lod.ZoneSize * TerrainChunkSize.RectSize.x,
                (int)Lod.ZoneSize * TerrainChunkSize.RectSize.y);

            var objects = new List<LodObject>();
            var gen = new ColumnGen(Sim);

            foreach (var tree in Sim.GetAreaTrees(wmin, wmax))
            {
                var sample = gen.Get((tree.Pos, (object)Index, (object?)null));
                if (sample == null) continue;

                var kind = TreeToObjectKind(tree.ForestKind);
                var pos = new int3(tree.Pos.x - wmin.x, tree.Pos.y - wmin.y, (int)sample.Alt);

                InstFlags flags = sample.SnowCover ? InstFlags.SnowCovered : 0;
                flags |= (InstFlags)(((int)(tree.Seed % 4)) << 2);

                var color = new Rgb<byte>(sample.StoneCol.R, sample.StoneCol.G, sample.StoneCol.B);
                objects.Add(new LodObject(kind, pos, flags, color));
            }

            return new Zone(objects);
        }

        private static ObjectKind TreeToObjectKind(ForestKind kind) => kind switch
        {
            ForestKind.Dead => ObjectKind.Dead,
            ForestKind.Pine => ObjectKind.Pine,
            ForestKind.Mangrove => ObjectKind.Mangrove,
            ForestKind.Acacia => ObjectKind.Acacia,
            ForestKind.Baobab => ObjectKind.Baobab,
            ForestKind.Birch => ObjectKind.Birch,
            ForestKind.Redwood => ObjectKind.Redwood,
            ForestKind.Palm => ObjectKind.Palm,
            ForestKind.Frostpine => ObjectKind.Frostpine,
            ForestKind.Giant => ObjectKind.GiantTree,
            _ => ObjectKind.GenericTree,
        };

        /// <summary>
        /// Determine a location name for the given world position. The search
        /// looks for the nearest site or point of interest.
        /// </summary>
        public string? GetLocationName(int2 wpos)
        {
            float bestDistSq = float.MaxValue;
            string? best = null;

            foreach (var (_, site) in Index.Sites.Enumerate())
            {
                float distSq = math.lengthsq(wpos - site.Position);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = site.Name;
                }

                foreach (var poi in site.PointsOfInterest)
                {
                    if (math.all(poi.Position == wpos))
                        return poi.Name;
                }
            }

            return best;
        }

        public IEnumerable<AirshipRoute> GetAirshipRoutes()
            => Index.Airships.Routes.Values;

        public IEnumerable<AirshipRoute> GetAirshipRoutesFrom(Store<Site.Site>.Id id)
            => Index.Airships.RoutesFrom(id);

    }
}
