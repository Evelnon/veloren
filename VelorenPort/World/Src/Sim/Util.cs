using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Sim
{
    /// <summary>
    /// Helper functions ported from world/src/sim/util.rs
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Calculate a factor representing how close a chunk index is to the edge of the map.
        /// Returns values in [0,1], where 1 is far from the edge.
        /// </summary>
        public static float MapEdgeFactor(MapSizeLg mapSizeLg, int posIndex)
        {
            int2 pos = mapSizeLg.UniformIdxAsVec2(posIndex);
            int2 size = mapSizeLg.Chunks;
            float2 f = new float2(
                (size.x / 2f - math.abs(pos.x - size.x / 2f)) / (16f / 1024f * size.x),
                (size.y / 2f - math.abs(pos.y - size.y / 2f)) / (16f / 1024f * size.y)
            );
            float r = math.min(f.x, f.y);
            return math.max(0f, math.min(1f, r));
        }

        /// <summary>
        /// Compute the CDF of the weighted sum of uniformly distributed variables.
        /// </summary>
        public static float CdfIrwinHall(float[] weights, float[] samples)
        {
            int n = weights.Length;
            if (samples.Length != n) throw new ArgumentException("samples length mismatch");

            double x = 0.0;
            for (int i = 0; i < n; i++) x += weights[i] * samples[i];

            double y = 0.0;
            int subsets = 1 << n;
            for (int subset = 0; subset < subsets; subset++)
            {
                int k = 0;
                double sum = 0.0;
                for (int i = 0; i < n; i++)
                {
                    if ((subset & (1 << i)) != 0)
                    {
                        k++;
                        sum += weights[i];
                    }
                }
                double z = Math.Pow(Math.Max(0.0, x - sum), n);
                y += (k % 2 == 0) ? z : -z;
            }
            double denom = 1.0;
            for (int i = 0; i < n; i++) denom *= weights[i];
            y /= denom;

            // divide by n!
            double fact = 1.0;
            for (int i = 2; i <= n; i++) fact *= i;
            return (float)(y / fact);
        }

        /// <summary>
        /// Evaluate <paramref name="func"/> for every chunk and return a uniformly
        /// distributed ranking of the results. Values that return null are skipped.
        /// </summary>
        public static ((float rank, float value)[] uniform, (int idx, float value)[] noise)
            UniformNoise(MapSizeLg mapSizeLg, Func<int, float2, float?> func)
        {
            int len = mapSizeLg.ChunksLen;
            var vals = new List<(int idx, float val)>();
            for (int i = 0; i < len; i++)
            {
                float2 pos = mapSizeLg.UniformIdxAsVec2(i) * TerrainConstants.ChunkSize;
                float? v = func(i, pos);
                if (v.HasValue && !float.IsNaN(v.Value))
                    vals.Add((i, v.Value));
            }

            vals.Sort((a, b) =>
            {
                int c = a.val.CompareTo(b.val);
                if (c == 0) c = a.idx.CompareTo(b.idx);
                return c;
            });

            var uniform = new (float rank, float value)[len];
            float total = vals.Count;
            for (int order = 0; order < vals.Count; order++)
            {
                var entry = vals[order];
                uniform[entry.idx] = ((order + 1) / total, entry.val);
            }

            return (uniform, vals.ToArray());
        }

        /// <summary>
        /// Determine which chunks of the world should be considered ocean based
        /// on a height function. Chunks connected to the map edge with
        /// non-positive altitude are marked as ocean.
        /// </summary>
        public static bool[] GetOceans(MapSizeLg mapSizeLg, Func<int, float> alt)
        {
            int len = mapSizeLg.ChunksLen;
            var result = new bool[len];
            var stack = new System.Collections.Generic.Stack<int>();

            void Push(int2 pos)
            {
                int idx = mapSizeLg.Vec2AsUniformIdx(pos);
                if (alt(idx) <= 0f)
                    stack.Push(idx);
            }

            int2 size = mapSizeLg.Chunks;
            for (int x = 0; x < size.x; x++)
            {
                Push(new int2(x, 0));
                Push(new int2(x, size.y - 1));
            }
            for (int y = 1; y < size.y - 1; y++)
            {
                Push(new int2(0, y));
                Push(new int2(size.x - 1, y));
            }

            while (stack.Count > 0)
            {
                int idx = stack.Pop();
                if (result[idx])
                    continue;
                result[idx] = true;

                int2 pos = mapSizeLg.UniformIdxAsVec2(idx);
                foreach (var dir in WorldUtil.CARDINALS)
                {
                    int2 n = pos + dir;
                    if (n.x < 0 || n.y < 0 || n.x >= size.x || n.y >= size.y)
                        continue;
                    int nidx = mapSizeLg.Vec2AsUniformIdx(n);
                    if (alt(nidx) <= 0f)
                        stack.Push(nidx);
                }
            }

            return result;
        }

        /// <summary>
        /// Enumerate neighboring chunk indices for the given index using an
        /// eight-connected neighborhood.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<int> Neighbors(MapSizeLg mapSizeLg, int idx)
        {
            int2 pos = mapSizeLg.UniformIdxAsVec2(idx);
            int2 size = mapSizeLg.Chunks;
            foreach (var dir in WorldUtil.NEIGHBORS)
            {
                int2 n = pos + dir;
                if (n.x < 0 || n.y < 0 || n.x >= size.x || n.y >= size.y)
                    continue;
                yield return mapSizeLg.Vec2AsUniformIdx(n);
            }
        }

        /// <summary>
        /// Compute the downhill map for all chunks. Values are the index of the
        /// lowest neighboring chunk, -1 for local minima, and -2 for ocean
        /// chunks.
        /// </summary>
        public static int[] Downhill(MapSizeLg mapSizeLg, Func<int, float> alt, Func<int, bool> isOcean)
        {
            int len = mapSizeLg.ChunksLen;
            var result = new int[len];
            for (int posi = 0; posi < len; posi++)
            {
                float nh = alt(posi);
                if (isOcean(posi))
                {
                    result[posi] = -2;
                    continue;
                }

                int best = -1;
                float besth = nh;
                foreach (int n in Neighbors(mapSizeLg, posi))
                {
                    float nbh = alt(n);
                    if (nbh < besth)
                    {
                        besth = nbh;
                        best = n;
                    }
                }
                result[posi] = best;
            }
            return result;
        }

        /// <summary>
        /// Iterate through all cells in a 7x7 neighbourhood around the given chunk index.
        /// This mirrors the behavior of Rust's local_cells function.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<int> LocalCells(MapSizeLg mapSizeLg, int idx)
        {
            int2 pos = mapSizeLg.UniformIdxAsVec2(idx);
            const int gridSize = 3;
            int gridBounds = gridSize * 2 + 1;
            for (int i = 0; i < gridBounds * gridBounds; i++)
            {
                int2 p = new int2(
                    pos.x + (i % gridBounds) - gridSize,
                    pos.y + (i / gridBounds) - gridSize
                );
                if (p.x < 0 || p.y < 0 || p.x >= mapSizeLg.Chunks.x || p.y >= mapSizeLg.Chunks.y)
                    continue;
                yield return mapSizeLg.Vec2AsUniformIdx(p);
            }
        }

        /// <summary>
        /// Enumerate neighbors whose downhill target equals <paramref name="idx"/>.
        /// </summary>
        public static System.Collections.Generic.IEnumerable<int> Uphill(MapSizeLg mapSizeLg, int[] downhill, int idx)
        {
            foreach (int n in Neighbors(mapSizeLg, idx))
                if (downhill[n] == idx)
                    yield return n;
        }
    }
}
