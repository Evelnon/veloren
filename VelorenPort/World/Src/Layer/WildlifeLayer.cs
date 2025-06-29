using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using static VelorenPort.NativeMath.math;

namespace VelorenPort.World.Layer;

public static class WildlifeLayer
{
    public delegate float DensityFunc(LayerContext ctx, float3 wpos, int top);

    private static readonly Dictionary<FaunaKind, DensityFunc> DensityFuncs = new()
    {
        { FaunaKind.Wolf, (ctx, pos, _) => math.saturate(ctx.Noise.Wildlife(pos * 0.12f) - 0.8f) },
        { FaunaKind.Bear, (ctx, pos, _) => ctx.Noise.Cave(pos * 0.05f) > 0.7f ? 0.05f : 0f },
        { FaunaKind.Deer, (ctx, pos, _) => ctx.Noise.Tree(pos * 0.07f) > 0.8f ? 0.3f : 0f },
        { FaunaKind.Rabbit, (ctx, pos, _) => ctx.Supplement.ResourceDeposits.Exists(d => !d.Depleted && math.length((float3)d.Position - pos) < 3f) ? 0.5f : 0.05f }
    };

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
            if (top <= 0 || top >= Chunk.Height - 1)
                continue;

            float3 wpos = new float3(
                chunk.Position.x * Chunk.Size.x + x,
                chunk.Position.y * Chunk.Size.y + y,
                top + 1);

            foreach (var (kind, func) in DensityFuncs)
            {
                float prob = func(ctx, wpos, top) * 0.1f;
                if (ctx.Rng.NextDouble() < prob)
                {
                    var spawn = new FaunaSpawn((int3)wpos, kind);
                    chunk.AddWildlife(spawn);
                    ctx.Supplement.Wildlife.Add(spawn);
                    ctx.Supplement.WildlifeEntities.Add(new WildlifeEntity(spawn.Position, spawn.Kind));
                    break;
                }
            }
        }

        foreach (var deposit in ctx.Supplement.ResourceDeposits)
        {
            if (deposit.Depleted || deposit.Amount <= 0f || ctx.Rng.NextDouble() > 0.25)
                continue;
            int3 pos = deposit.Position + new int3(0, 0, 1);
            var spawn = new FaunaSpawn(pos, FaunaKind.Rabbit);
            chunk.AddWildlife(spawn);
            ctx.Supplement.Wildlife.Add(spawn);
            ctx.Supplement.WildlifeEntities.Add(new WildlifeEntity(spawn.Position, spawn.Kind));
        }
    }
}
