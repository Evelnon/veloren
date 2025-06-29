using VelorenPort.NativeMath;

namespace VelorenPort.World.Layer;

/// <summary>
/// Very simplified rock placement mimicking the Rust implementation.
/// Creates small piles of weak rock on the surface.
/// </summary>
public static class RockLayer
{
    public static void Apply(LayerContext ctx, Chunk chunk)
    {
        for (int x = 0; x < Chunk.Size.x; x++)
        for (int y = 0; y < Chunk.Size.y; y++)
        {
            int top = -1;
            for (int z = Chunk.Height - 1; z >= 0; z--)
            {
                if (chunk[x, y, z].IsFilled)
                {
                    top = z;
                    break;
                }
            }
            if (top <= 0 || top >= Chunk.Height - 2)
                continue;

            float3 wpos = new float3(
                chunk.Position.x * Chunk.Size.x + x,
                chunk.Position.y * Chunk.Size.y + y,
                top);
            float n = ctx.Noise.Ore(wpos * 0.09f);
            if (n > 0.82f)
                chunk[x, y, top + 1] = new Block(BlockKind.WeakRock);
        }
    }
}
