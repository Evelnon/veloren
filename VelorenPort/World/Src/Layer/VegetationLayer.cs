using VelorenPort.NativeMath;

namespace VelorenPort.World.Layer;

/// <summary>
/// Combined shrub and tree logic roughly mirroring the Rust behaviour.
/// </summary>
public static class VegetationLayer
{
    public static void Apply(LayerContext ctx, Chunk chunk)
    {
        ApplyShrub(ctx, chunk);
        ApplyTree(ctx, chunk);
    }

    private static void ApplyShrub(LayerContext ctx, Chunk chunk)
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
            float n = ctx.Noise.Shrub(wpos * 0.12f);
            if (n > 0.7f)
                chunk[x, y, top + 1] = new Block(BlockKind.Leaves);
        }
    }

    private static void ApplyTree(LayerContext ctx, Chunk chunk)
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
            if (top <= 0 || top >= Chunk.Height - 4)
                continue;

            float3 wpos = new float3(
                chunk.Position.x * Chunk.Size.x + x,
                chunk.Position.y * Chunk.Size.y + y,
                top);
            float n = ctx.Noise.Tree(wpos * 0.07f);
            if (n > 0.78f)
            {
                for (int t = 1; t <= 3; t++)
                    chunk[x, y, top + t] = new Block(BlockKind.Wood);

                int leafZ = top + 3;
                chunk[x, y, leafZ + 1] = new Block(BlockKind.Leaves);
                if (x > 0) chunk[x - 1, y, leafZ] = new Block(BlockKind.Leaves);
                if (x < Chunk.Size.x - 1) chunk[x + 1, y, leafZ] = new Block(BlockKind.Leaves);
                if (y > 0) chunk[x, y - 1, leafZ] = new Block(BlockKind.Leaves);
                if (y < Chunk.Size.y - 1) chunk[x, y + 1, leafZ] = new Block(BlockKind.Leaves);
            }
        }
    }
}
