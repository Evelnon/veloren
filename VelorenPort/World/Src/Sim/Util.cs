using System;
using Unity.Mathematics;

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
    }
}
