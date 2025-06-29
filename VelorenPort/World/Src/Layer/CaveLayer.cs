using VelorenPort.NativeMath;

namespace VelorenPort.World.Layer;

public static class CaveLayer
{
    /// <summary>
    /// Number of cavern levels generated below the surface. Roughly mirrors
    /// the Rust constant `LAYERS` but greatly simplified.
    /// </summary>
    private const int Layers = 3;

    /// <summary>
    /// Generate a network of caverns. Multiple depth layers are carved using
    /// noise and simple humidity/temperature checks inspired by the Rust
    /// implementation.
    /// </summary>
    public static void Apply(LayerContext ctx, Chunk chunk)
    {
        int step = Chunk.Height / (Layers + 1);
        for (int level = 0; level < Layers; level++)
        {
            int centerZ = step * (level + 1);
            int half = step / 2;

            for (int x = 0; x < Chunk.Size.x; x++)
            for (int y = 0; y < Chunk.Size.y; y++)
            for (int z = math.max(1, centerZ - half); z < math.min(centerZ + half, Chunk.Height / 2); z++)
            {
                float3 wpos = new float3(
                    chunk.Position.x * Chunk.Size.x + x,
                    chunk.Position.y * Chunk.Size.y + y,
                    z);

                // Base cave noise plus a little fbm for variation
                float n = ctx.Noise.Cave(wpos * 0.08f) + ctx.Noise.CaveFbm(wpos * 0.02f) * 0.5f;

                if (n <= 0.6f)
                    continue;

                // Very rough humidity and temperature approximation. Values
                // roughly in the range [0,1] and [-1,1] respectively.
                float humidity = math.saturate(ctx.Noise.Scatter(wpos * 0.01f) * 0.5f + 0.5f);
                float temp = ctx.Noise.Tree(wpos * 0.01f);

                // Only carve caverns when the local climate is reasonably
                // hospitable. This mimics checks in the Rust version without
                // referencing the full biome system.
                if (humidity > 0.2f && humidity < 0.9f && temp > -1.5f)
                {
                    chunk[x, y, z] = Block.Air;
                }
            }
        }
    }
}
