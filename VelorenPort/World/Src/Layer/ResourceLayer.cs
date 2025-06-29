using VelorenPort.NativeMath;

namespace VelorenPort.World.Layer;

public static class ResourceLayer
{
    public static void Apply(LayerContext ctx, Chunk chunk)
    {
        for (int x = 0; x < Chunk.Size.x; x++)
        for (int y = 0; y < Chunk.Size.y; y++)
        for (int z = 1; z < Chunk.Height / 2; z++)
        {
            float3 wpos = new float3(
                chunk.Position.x * Chunk.Size.x + x,
                chunk.Position.y * Chunk.Size.y + y,
                z);
            float n = ctx.Noise.Ore(wpos * 0.12f);
            if (n > 0.86f)
            {
                BlockKind kind = n > 0.93f ? BlockKind.GlowingRock : BlockKind.GlowingWeakRock;
                chunk[x, y, z] = new Block(kind);
            }
        }
    }
}
