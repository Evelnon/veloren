using System.Collections.Generic;
using System.Linq;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Sim;

/// <summary>
/// Simplified erosion model inspired by world/src/sim/erosion.rs. Water flux is
/// accumulated from uphill neighbors and used to erode terrain along the
/// steepest descent.
/// </summary>
public static class Erosion
{
    public static void Apply(WorldSim sim, int iterations = 1, float k = 0.01f)
    {
        var size = sim.GetSize();
        int len = size.x * size.y;
        var alt = new float[len];
        var downhill = new int[len];
        var flux = new float[len];

        for (int y = 0; y < size.y; y++)
        for (int x = 0; x < size.x; x++)
        {
            var chunk = sim.Get(new int2(x, y));
            alt[y * size.x + x] = chunk?.Alt ?? 0f;
        }

        for (int step = 0; step < iterations; step++)
        {
            // Compute downhill map
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                int idx = y * size.x + x;
                float best = alt[idx];
                int bestIdx = -1;
                foreach (var d in WorldUtil.NEIGHBORS)
                {
                    int2 np = new int2(x + d.x, y + d.y);
                    if (np.x < 0 || np.y < 0 || np.x >= size.x || np.y >= size.y)
                        continue;
                    int nidx = np.y * size.x + np.x;
                    if (alt[nidx] < best)
                    {
                        best = alt[nidx];
                        bestIdx = nidx;
                    }
                }
                downhill[idx] = bestIdx;
            }

            // Sort by altitude
            var order = Enumerable.Range(0, len).OrderBy(i => alt[i]).ToArray();
            for (int i = 0; i < len; i++) flux[i] = 1f;

            foreach (int idx in order)
            {
                int d = downhill[idx];
                if (d >= 0) flux[d] += flux[idx];
            }

            for (int idx = 0; idx < len; idx++)
            {
                int d = downhill[idx];
                if (d < 0) continue;
                float slope = alt[idx] - alt[d];
                alt[idx] -= k * flux[idx] * slope;
            }
        }

        for (int y = 0; y < size.y; y++)
        for (int x = 0; x < size.x; x++)
        {
            var chunk = sim.Get(new int2(x, y));
            if (chunk != null)
                chunk.Alt = alt[y * size.x + x];
        }
    }

    /// <summary>
    /// Raise local minima so water can flow between chunks. This performs a very
    /// naive sink filling pass over the currently loaded chunks.
    /// </summary>
    public static void FillSinks(WorldSim sim, float epsilon = 0.01f)
    {
        bool changed;
        do
        {
            changed = false;
            foreach (var (pos, chunk) in sim.Chunks)
            {
                float target = chunk.Alt;
                foreach (var off in WorldUtil.NEIGHBORS)
                {
                    var n = sim.Get(pos + off);
                    if (n != null)
                        target = math.min(target, n.Alt + epsilon);
                }
                if (target > chunk.Alt + epsilon)
                {
                    chunk.Alt = target;
                    changed = true;
                }
            }
        } while (changed);
    }
}
