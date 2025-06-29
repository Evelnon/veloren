using VelorenPort.NativeMath;
using static VelorenPort.NativeMath.math;

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
            float n = ctx.Noise.Ore(wpos * 0.1f);
            if (n > 0.92f)
            {
                int cluster = n > 0.97f ? 3 : 2;
                BlockKind kind = n > 0.97f ? BlockKind.GlowingRock : BlockKind.GlowingWeakRock;
                for (int i = 0; i < cluster; i++)
                {
                    int dx = (int)math.floor(ctx.Noise.Ore(new float3(x + i, y, z) * 0.2f) * 3f) - 1;
                    int dy = (int)math.floor(ctx.Noise.Ore(new float3(x, y + i, z) * 0.2f) * 3f) - 1;
                    int dz = (int)math.floor(ctx.Noise.Ore(new float3(x, y, z + i) * 0.2f) * 3f) - 1;
                    int3 pos = new int3(
                        clamp(x + dx, 0, Chunk.Size.x - 1),
                        clamp(y + dy, 0, Chunk.Size.y - 1),
                        clamp(z + dz, 1, Chunk.Height / 2 - 1));
                    chunk[pos.x, pos.y, pos.z] = new Block(kind);
                    ctx.Supplement.ResourceBlocks.Add(pos);
                    ctx.Supplement.ResourceDeposits.Add(new ResourceDeposit(pos, kind));
                }
            }
        }
    }
}
