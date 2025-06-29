using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.World.Sim;

namespace VelorenPort.World {
    /// <summary>
    /// Core world simulation. Most functionality is pending port from Rust.
    /// </summary>
    [Serializable]
    public class WorldSim {
        private readonly Noise _noise;
        private readonly int2 _size;
        private readonly Dictionary<int2, SimChunk> _chunks = new();
        private readonly RegionMap _regions = new();
        private readonly StructureGen2d _structureGen;
        private readonly Sim.HumidityMap _humidity;

        public WorldSim(uint seed, int2 size) {
            _noise = new Noise(seed);
            _size = size;
            _structureGen = new StructureGen2d(seed, 24, 10);
            _humidity = Sim.HumidityMap.Generate(size);

        }

        public static WorldSim Empty() => new WorldSim(0, int2.zero);

        public int2 GetSize() => _size;

        public T GetInterpolated<T>(int2 wpos, Func<SimChunk, T> f) where T : struct {
            var chunk = GetWpos(wpos);
            return chunk == null ? default : f(chunk);
        }

        public float GetSurfaceAltApprox(int2 wpos)
        {
            var chunk = GetWpos(wpos);
            if (chunk == null) return WorldDefaults.CONFIG.SeaLevel;
            return math.max(chunk.Alt, chunk.WaterAlt);
        }

        public float? GetAltApprox(int2 wpos) => GetWpos(wpos)?.Alt;

        public SimChunk? Get(int2 chunkPos) {
            if (_chunks.TryGetValue(chunkPos, out var chunk)) return chunk;
            return GenerateChunk(chunkPos);
        }

        public void Set(int2 chunkPos, SimChunk chunk) {
            _chunks[chunkPos] = chunk;
        }

        public bool Remove(int2 chunkPos) => _chunks.Remove(chunkPos);

        public void SetWpos(int2 wpos, SimChunk chunk) => Set(TerrainChunkSize.WposToCpos(wpos), chunk);

        public SimChunk? GetWpos(int2 wpos) => Get(TerrainChunkSize.WposToCpos(wpos));

        public float? GetGradientApprox(int2 wpos) {
            const int SAMP_RES = 8;
            var altx0 = GetAltApprox(wpos - new int2(SAMP_RES, 0)) ?? 0f;
            var altx1 = GetAltApprox(wpos + new int2(SAMP_RES, 0)) ?? 0f;
            var alty0 = GetAltApprox(wpos - new int2(0, SAMP_RES)) ?? 0f;
            var alty1 = GetAltApprox(wpos + new int2(0, SAMP_RES)) ?? 0f;
            return math.length(new float2(altx1 - altx0, alty1 - alty0)) / SAMP_RES;
        }

        public (float dist, float2 pos, Path path, float2 tangent)? GetNearestPath(int2 wpos)
        {
            int2 cpos = TerrainChunkSize.WposToCpos(wpos);
            float2 wposf = (float2)wpos;
            float bestDistSq = float.MaxValue;
            float2 bestPos = float2.zero;
            Path bestPath = Path.Default;
            float2 bestTangent = float2.zero;

            foreach (var ctrl in WorldUtil.LOCALITY)
            {
                var chunk = Get(cpos + ctrl);
                if (chunk == null) continue;
                var way = chunk.Path.way;
                if (way.Neighbors == 0) continue;

                float2 ctrlPos = (float2)TerrainChunkSize.CposToWposCenter(cpos + ctrl) + (float2)way.Offset;

                for (int i = 0; i < WorldUtil.NEIGHBORS.Length; i++)
                {
                    if ((way.Neighbors & (1 << i)) == 0) continue;
                    int2 npos = cpos + ctrl + WorldUtil.NEIGHBORS[i];
                    var nChunk = Get(npos);
                    if (nChunk == null) continue;
                    var nWay = nChunk.Path.way;
                    float2 nPos = (float2)TerrainChunkSize.CposToWposCenter(npos) + (float2)nWay.Offset;

                    float2 dir = nPos - ctrlPos;
                    float lenSq = math.lengthsq(dir);
                    if (lenSq < 0.0001f) continue;
                    float t = math.clamp(math.dot(wposf - ctrlPos, dir) / lenSq, 0f, 1f);
                    float2 pos = ctrlPos + dir * t;
                    float distSq = math.lengthsq(pos - wposf);
                    if (distSq < bestDistSq)
                    {
                        bestDistSq = distSq;
                        bestPos = pos;
                        bestPath = chunk.Path.path;
                        bestTangent = math.normalize(dir);
                    }
                }
            }

            if (bestDistSq == float.MaxValue) return null;
            return (math.sqrt(bestDistSq), bestPos, bestPath, bestTangent);
        }

        public IEnumerable<int2> LoadedChunks => _chunks.Keys;

        public Noise Noise => _noise;

        /// <summary>Collection of regions with entity membership.</summary>
        public RegionMap Regions => _regions;
        public Sim.HumidityMap Humidity => _humidity;

        /// <summary>Advance simulation state. Currently only ticks regions.</summary>
        public void Tick(float dt) {
            _regions.Tick();
            _humidity.Diffuse();
            Sim.Erosion.Apply(this);

        }

        public float[,] GetAltitudeMap(int2 cpos, int radius) {
            int size = radius * 2 + 1;
            var map = new float[size, size];
            for (int dy = -radius; dy <= radius; dy++)
            for (int dx = -radius; dx <= radius; dx++) {
                var pos = cpos + new int2(dx, dy);
                map[dx + radius, dy + radius] = Get(pos)?.Alt ?? 0f;
            }
            return map;
        }

        /// <summary>
        /// Enumerate all loaded chunks with their coordinates.
        /// </summary>
        public IEnumerable<(int2 pos, SimChunk chunk)> Chunks
        {
            get
            {
                foreach (var kv in _chunks)
                    yield return (kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// Return basic information about nearby regions surrounding
        /// <paramref name="chunkPos"/>. This is a very small subset of the
        /// more complex region queries in the original Rust version and is
        /// intended only for testing other systems.
        /// </summary>
        public IEnumerable<RegionInfo> GetNearRegions(int2 chunkPos, int radius)
        {
            for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
            {
                var pos = chunkPos + new int2(x, y);
                float dist = math.length(new float2(x, y));
                uint seed = (uint)math.hash(pos);
                yield return new RegionInfo(
                    pos,
                    pos * TerrainChunkSize.RectSize,
                    dist,
                    seed);
            }
        }

        public Lottery<ForestKind?> MakeForestLottery(int2 wpos)
        {
            var chunk = GetWpos(wpos);
            if (chunk == null)
                return new Lottery<ForestKind?>(new[] { (1f, (ForestKind?)null) });

            var env = new Environment
            {
                Humid = chunk.Humidity,
                Temp = chunk.Temp,
                NearWater = math.saturate(math.abs(chunk.WaterAlt - chunk.Alt) / 64f)
            };

            const double CLUSTER_SIZE = 48.0;
            var items = new List<(float, ForestKind?)>();
            int i = 0;
            foreach (ForestKind fk in Enum.GetValues(typeof(ForestKind)))
            {
                var nz = new FastNoise2d((uint)(i * 37)).Get((double2)wpos / CLUSTER_SIZE);
                nz = (nz + 1f) * 0.5f;
                float weight = fk.Proclivity(env) * nz;
                items.Add((weight, fk));
                i++;
            }
            items.Add((0.001f, (ForestKind?)null));
            return new Lottery<ForestKind?>(items);
        }

        public IEnumerable<TreeAttr> GetNearTrees(int2 wpos)
        {
            foreach (var (pos, seed) in _structureGen.Get(wpos))
            {
                var lot = MakeForestLottery(pos);
                var kind = lot.ChooseSeeded(seed);
                if (kind.HasValue)
                    yield return new TreeAttr
                    {
                        Pos = pos,
                        Seed = seed,
                        Scale = 1f,
                        ForestKind = kind.Value,
                        Inhabited = false
                    };
            }
        }

        public IEnumerable<TreeAttr> GetAreaTrees(int2 wposMin, int2 wposMax)
        {
            foreach (var (pos, seed) in _structureGen.Iter(wposMin, wposMax))
            {
                var lot = MakeForestLottery(pos);
                var kind = lot.ChooseSeeded(seed);
                if (kind.HasValue)
                    yield return new TreeAttr
                    {
                        Pos = pos,
                        Seed = seed,
                        Scale = 1f,
                        ForestKind = kind.Value,
                        Inhabited = false
                    };
            }
        }

        public float3? ApproxChunkTerrainNormal(int2 chunkPos)
        {
            var curr = Get(chunkPos);
            if (curr == null || curr.Downhill == null)
                return null;

            var downPos = TerrainChunkSize.WposToCpos(curr.Downhill.Value);
            var down = Get(downPos);
            if (down == null)
                return null;

            if (math.abs(curr.Alt - down.Alt) < 0.0001f)
                return new float3(0f, 0f, 1f);

            float3 currPos = new float3(TerrainChunkSize.CposToWposCenter(chunkPos), curr.Alt);
            float3 downPos3 = new float3(TerrainChunkSize.CposToWposCenter(downPos), down.Alt);
            float3 downwards = currPos - downPos3;
            float3 flat = new float3(downwards.x, downwards.y, 0f);
            float3 res = math.cross(math.cross(downwards, flat), downwards);
            return math.normalize(res);
        }

        private SimChunk GenerateChunk(int2 chunkPos) {
            var worldPos = TerrainChunkSize.CposToWposCenter(chunkPos);
            float baseAlt = _noise.CaveFbm(new float3(worldPos.x * 0.01f, worldPos.y * 0.01f, 0));
            var chunk = new SimChunk {
                Alt = baseAlt * 32f,
                Basement = baseAlt * 31f,
                WaterAlt = 0f,
                Chaos = _noise.Scatter(new float3(worldPos, 0)) * 4f,
                Temp = _noise.Cave(new float3(worldPos, 1)) * 0.5f,
                Humidity = _noise.Scatter(new float3(worldPos, 2)) * 0.5f + 0.5f,
                Rockiness = math.abs(_noise.Cave(new float3(worldPos, 3))),
                TreeDensity = math.saturate(_noise.Scatter(new float3(worldPos,4)) * 0.5f + 0.5f),
                ForestKind = ForestKind.Oak,
                SpawnRate = 1f,
                River = new RiverData(),
                SurfaceVeg = 1f,
                Path = (new Way(), Path.Default),
                CliffHeight = 1f,
                Downhill = TerrainChunkSize.CposToWpos(chunkPos + new int2(0, 1))
            };

            chunk.Spot = Layer.SpotGenerator.Generate(chunkPos, _noise);
            _chunks[chunkPos] = chunk;
            Humidity.Set(chunkPos, chunk.Humidity);
            return chunk;
        }
    }
}
