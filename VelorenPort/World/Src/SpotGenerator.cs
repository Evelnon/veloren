using System;
using Unity.Mathematics;

namespace VelorenPort.World;

/// <summary>
/// Simple placement of <see cref="Spot"/> values across the world simulation.
/// This does not load any external manifests and only tags chunks with a random
/// spot type.
/// </summary>
public static class SpotGenerator
{
    /// <summary>Assign random spots to chunks according to the given density.</summary>
    /// <param name="sim">World simulation to modify.</param>
    /// <param name="density">Approximate fraction of chunks that receive a spot.</param>
    /// <param name="rng">Optional RNG instance for deterministic placement.</param>
public static void Generate(
    WorldSim sim,
    float density = 0.002f,
    Random? rng = null,
    SpotManifest? manifest = null)
{
    rng ??= new Random();
    int2 size = sim.GetSize();
    if (manifest != null && manifest.Spots.Count > 0)
    {
        foreach (var (spot, props) in manifest.Spots)
        {
            if (!props.Spawn) continue;
            int total = (int)(size.x * size.y * density * props.Freq);
            for (int i = 0; i < total; i++)
            {
                int2 pos = new int2(rng.Next(0, size.x), rng.Next(0, size.y));
                var chunk = sim.Get(pos);
                if (chunk == null || chunk.Spot != null)
                    continue;
                chunk.Spot = spot;
            }
        }
    }
    else
    {
        int total = (int)(size.x * size.y * density);
        var spots = Enum.GetValues(typeof(Spot));
        for (int i = 0; i < total; i++)
        {
            int2 pos = new int2(rng.Next(0, size.x), rng.Next(0, size.y));
            var chunk = sim.Get(pos);
            if (chunk == null || chunk.Spot != null)
                continue;
            chunk.Spot = (Spot)spots.GetValue(rng.Next(spots.Length))!;
        }
    }
}
}
