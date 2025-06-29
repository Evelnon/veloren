using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World {
    /// <summary>
    /// Simple search configuration mirroring <c>SearchCfg</c> in the Rust code.
    /// </summary>
    [Serializable]
    public struct SearchCfg {
        public float PathDiscount;
        public float GradientAversion;
        public float EdgeAversion;
        public float AltCostWeight;
        public Dictionary<ChunkResource, float>? ResourceCosts;

        public SearchCfg(
            float pathDiscount,
            float gradientAversion,
            float edgeAversion = 0f,
            float altCostWeight = 0f,
            Dictionary<ChunkResource, float>? resourceCosts = null)
        {
            PathDiscount = pathDiscount;
            GradientAversion = gradientAversion;
            EdgeAversion = edgeAversion;
            AltCostWeight = altCostWeight;
            ResourceCosts = resourceCosts;
        }
    }

    /// <summary>
    /// Stub pathfinder used by world generation.
    /// </summary>
    public struct Searcher {
        private readonly Land _land;
        private readonly Func<int2, float>? _extraCost;
        private readonly MapSizeLg? _mapSize;
        private readonly Func<int2, bool>? _passable;
        private readonly NavGrid? _navGrid;
        private readonly NavMesh? _navMesh;
        public SearchCfg Cfg { get; }

        public Searcher(
            Land land,
            SearchCfg cfg,
            Func<int2, float>? extraCost = null,
            Func<int2, bool>? passable = null,
            NavGrid? navGrid = null,
            NavMesh? navMesh = null)
        {
            _land = land;
            Cfg = cfg;
            _extraCost = extraCost;
            _passable = passable;
            _navGrid = navGrid;
            _navMesh = navMesh;
            _mapSize = TryMapSize(land.Size);
        }

        /// <summary>
        /// Expose resource information for external AI systems.
        /// </summary>
        public System.Collections.Generic.Dictionary<ChunkResource, float> GetResources(int2 chunkPos)
            => _land.GetChunkResources(chunkPos);

        private static MapSizeLg? TryMapSize(int2 size)
        {
            if (size.x <= 0 || size.y <= 0)
                return null;
            int Log2Ceil(int v)
            {
                int r = 0;
                int p = 1;
                while (p < v)
                {
                    p <<= 1;
                    r++;
                }
                return r;
            }
            return new MapSizeLg(new int2(Log2Ceil(size.x), Log2Ceil(size.y)));

        }

        private static readonly int2[] DIRS =
        {
            new int2(1, 0), new int2(-1, 0), new int2(0, 1), new int2(0, -1),
            new int2(1, 1), new int2(1, -1), new int2(-1, 1), new int2(-1, -1)
        };

        private IEnumerable<(int2 Node, float Cost)> Neighbors(int2 pos)
        {
            IEnumerable<int2> list;
            if (_navMesh != null)
            {
                list = _navMesh.GetNeighbors(pos);
            }
            else
            {
                var temp = new int2[DIRS.Length];
                for (int i = 0; i < DIRS.Length; i++)
                    temp[i] = pos + DIRS[i];
                list = temp;
            }

            foreach (var next in list)
            {
                if (_navGrid != null && _navGrid.IsBlocked(next))
                    continue;
                if (_passable != null && !_passable(next))
                    continue;

                yield return (next, NeighborCost(pos, next));
            }
        }

        private float NeighborCost(int2 pos, int2 next)
        {
            int2 dir = next - pos;
            float baseCost = math.length((float2)dir);
            int2 wpos = TerrainChunkSize.CposToWposCenter(next);
            float grad = _land.GetGradientApprox(wpos);
            bool path = _land.GetChunk(next)?.Path.way.IsWay ?? false;
            float extra = 0f;
            if (_mapSize.HasValue && Cfg.EdgeAversion > 0f)
            {
                var size = _mapSize.Value.Chunks;
                int2 clamped = new int2(math.clamp(next.x, 0, size.x - 1), math.clamp(next.y, 0, size.y - 1));
                int idx = _mapSize.Value.Vec2AsUniformIdx(clamped);
                float factor = Sim.Util.MapEdgeFactor(_mapSize.Value, idx);
                extra += (1f - factor) * Cfg.EdgeAversion;
            }
            if (_extraCost != null)
                extra += _extraCost(next);

            Dictionary<ChunkResource, float>? weights = Cfg.ResourceCosts;
            if (weights == null)
                weights = ChunkResourceUtil.DefaultWeights;
            if (weights != null)
            {
                var res = _land.GetChunkResources(next);
                foreach (var kv in res)
                    if (weights.TryGetValue(kv.Key, out var w))
                        extra += kv.Value * w;
            }

            float mult = 1f + grad * Cfg.GradientAversion;
            if (path)
                mult *= math.max(0f, 1f - Cfg.PathDiscount);

            float cost = baseCost * mult + extra;
            if (Cfg.AltCostWeight > 0f)
            {
                int2 curWpos = TerrainChunkSize.CposToWposCenter(pos);
                float altA = _land.GetSurfaceAltApprox(curWpos);
                float altB = _land.GetSurfaceAltApprox(wpos);
                cost += math.abs(altA - altB) * Cfg.AltCostWeight;
            }
            return cost;
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

    /// <summary>
    /// Utility helpers for pathfinding structures.
    /// </summary>
    public static class Pathfinding
    {
        /// <summary>
        /// Build a simple navmesh directly from <see cref="WorldSim"/> data.
        /// Chunks with water above the surface or with gradient larger than
        /// <paramref name="maxGradient"/> are considered non walkable.
        /// </summary>
        public static NavMesh BuildNavMesh(WorldSim sim, float maxGradient = 1f)
        {
            var size = sim.GetSize();
            var grid = new NavGrid(size);
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                var pos = new int2(x, y);
                var chunk = sim.Get(pos);
                bool blocked = true;
                if (chunk != null)
                {
                    float grad = sim.GetGradientApprox(TerrainChunkSize.CposToWposCenter(pos)) ?? 0f;
                    blocked = chunk.WaterAlt > chunk.Alt || grad > maxGradient;
                }
                grid.SetBlocked(pos, blocked);
            }

            return NavMesh.Generate(grid);
        }
    }
}
