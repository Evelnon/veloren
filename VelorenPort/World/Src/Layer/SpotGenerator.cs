using System;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Layer;

/// <summary>
/// Very small scale spot generator selecting a random <see cref="Spot"/>
/// based on noise. This is only a placeholder until the real manifest
/// loading system is ported.
/// </summary>
public static class SpotGenerator
{
    public static Spot? Generate(int2 chunkPos, Noise noise)
    {
        float n = noise.Scatter(new float3(chunkPos.x * 0.05f, chunkPos.y * 0.05f, 42f));
        if (n > 0.8f)
        {
            var values = Enum.GetValues<Spot>();
            int idx = (int)math.floor((n - 0.8f) * values.Length) % values.Length;
            return values[idx];
        }
        return null;
    }
}
