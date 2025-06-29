using VelorenPort.NativeMath;

namespace VelorenPort.World.Sim;

/// <summary>
/// Simple topographic diffusion step approximating the algorithm in
/// world/src/sim/diffusion.rs. Heights are smoothed based on neighbouring
/// chunks.
/// </summary>
public static class Diffusion
{
    public static void Apply(WorldSim sim, float dt = 1f, float kd = 0.1f)
    {
        var size = sim.GetSize();
        var next = new float[size.x, size.y];

        for (int y = 0; y < size.y; y++)
        for (int x = 0; x < size.x; x++)
        {
            var pos = new int2(x, y);
            var chunk = sim.Get(pos);
            float alt = chunk?.Alt ?? 0f;
            float sum = 0f;
            int count = 0;
            foreach (var d in WorldUtil.CARDINALS)
            {
                var n = sim.Get(pos + d);
                if (n != null)
                {
                    sum += n.Alt - alt;
                    count++;
                }
            }
            next[x, y] = alt + kd * dt * sum;
        }

        for (int y = 0; y < size.y; y++)
        for (int x = 0; x < size.x; x++)
        {
            var chunk = sim.Get(new int2(x, y));
            if (chunk != null)
            {
                chunk.Alt = next[x, y];
            }
        }
    }
}
