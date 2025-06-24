using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Helper methods to iterate voxels along a ray using a 3D DDA algorithm.
    /// This is a simplified version of util::ray from the original project.
    /// </summary>
    [Serializable]
    public static class Ray {
        public static IEnumerable<int3> Cast(int3 from, int3 to, int maxIter = 100) {
            float3 pos = from;
            float3 dir = math.normalize((float3)to - (float3)from);
            float max = math.length((float3)to - (float3)from);
            float dist = 0f;
            const float EPS = 0.001f;

            for (int iter = 0; iter < maxIter && dist <= max; iter++) {
                int3 ipos = (int3)math.floor(pos);
                yield return ipos;

                float3 cellBoundary = math.select(0f, 1f, dir > 0f) - math.frac(pos);
                float3 delta = cellBoundary / math.max(math.abs(dir), new float3(EPS));
                float step = math.cmin(delta);
                if (step <= EPS) step = EPS;
                dist += step;
                pos = from + dir * dist;
            }
        }
    }
}
