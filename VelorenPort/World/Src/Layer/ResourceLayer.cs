using VelorenPort.NativeMath;
using static VelorenPort.NativeMath.math;

namespace VelorenPort.World.Layer;

public static class ResourceLayer
{
    private enum DepositShape { Sphere, Vein }

    private struct DepositConfig
    {
        public BlockKind Kind;
        public DepositShape Shape;
        public float Threshold;
        public int Size;
    }

    // Order matters: rarer deposits should appear first
    private static readonly DepositConfig[] Configs = new[]
    {
        new DepositConfig { Kind = BlockKind.GlowingRock, Shape = DepositShape.Vein, Threshold = 0.97f, Size = 4 },
        new DepositConfig { Kind = BlockKind.GlowingWeakRock, Shape = DepositShape.Sphere, Threshold = 0.92f, Size = 2 }
    };

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
            float depth = (float)z / (Chunk.Height / 2);

            foreach (var cfg in Configs)
            {
                // Adjust threshold slightly with depth so deeper deposits are a bit more common
                float t = cfg.Threshold - depth * 0.05f;
                if (n > t)
                {
                    int3 pos = new int3(x, y, z);
                    AddDeposit(chunk, ctx, pos, cfg);
                    break;
                }
            }
        }
    }

    private static void AddDeposit(Chunk chunk, LayerContext ctx, int3 pos, DepositConfig cfg)
    {
        switch (cfg.Shape)
        {
            case DepositShape.Sphere:
                AddSphere(chunk, ctx, pos, cfg.Size, cfg.Kind);
                break;
            case DepositShape.Vein:
                AddVein(chunk, ctx, pos, cfg.Size, cfg.Kind);
                break;
        }
    }

    private static void AddSphere(Chunk chunk, LayerContext ctx, int3 center, int radius, BlockKind kind)
    {
        for (int dx = -radius; dx <= radius; dx++)
        for (int dy = -radius; dy <= radius; dy++)
        for (int dz = -radius; dz <= radius; dz++)
        {
            if (math.lengthsq(new float3(dx, dy, dz)) > radius * radius + 0.1f)
                continue;
            int3 p = new int3(
                clamp(center.x + dx, 0, Chunk.Size.x - 1),
                clamp(center.y + dy, 0, Chunk.Size.y - 1),
                clamp(center.z + dz, 1, Chunk.Height / 2 - 1));
            chunk[p.x, p.y, p.z] = new Block(kind);
            ctx.Supplement.ResourceBlocks.Add(p);
        }
        ctx.Supplement.ResourceDeposits.Add(new ResourceDeposit(center, kind));
    }

    private static void AddVein(Chunk chunk, LayerContext ctx, int3 start, int length, BlockKind kind)
    {
        int axis = (int)math.abs(math.floor(ctx.Noise.Ore((float3)start * 0.2f) * 3f)) % 3;
        int3 dir = axis == 0 ? new int3(1, 0, 0) : axis == 1 ? new int3(0, 1, 0) : new int3(0, 0, 1);

        for (int i = 0; i < length; i++)
        {
            int3 p = new int3(
                clamp(start.x + dir.x * i, 0, Chunk.Size.x - 1),
                clamp(start.y + dir.y * i, 0, Chunk.Size.y - 1),
                clamp(start.z + dir.z * i, 1, Chunk.Height / 2 - 1));
            chunk[p.x, p.y, p.z] = new Block(kind);
            ctx.Supplement.ResourceBlocks.Add(p);
        }

        ctx.Supplement.ResourceDeposits.Add(new ResourceDeposit(start, kind));
    }
}
