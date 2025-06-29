using System;
using Unity.Mathematics;
using VelorenPort.World;

namespace VelorenPort.World.Layer
{
    /// <summary>
    /// Types of procedural layers applied to terrain.
    /// This is a minimal subset of the Rust implementation
    /// used for scattering objects and caves.
    /// </summary>
    [Serializable]
    public enum LayerType
    {
        Cave,
        Scatter,
        Shrub,
        Tree,
        Wildlife,
        OreVein
    }

    /// <summary>
    /// Simplified layer application context.
    /// </summary>
    public class LayerContext
    {
        public int2 ChunkPos { get; init; }
        public Random Rng { get; } = new();
        public Canvas? Canvas { get; init; }
    }

    /// <summary>
    /// Manager responsible for applying layers to world chunks.
    /// </summary>
    public static class LayerManager
    {
        /// <summary>
        /// Apply the requested layer to the chunk. Currently only logs the action.
        /// </summary>
        public static void Apply(LayerType layer, LayerContext ctx)
        {
            switch (layer)
            {
                case LayerType.Cave:
                    ApplyCaves(ctx);
                    break;
                case LayerType.Scatter:
                    ApplyScatter(ctx);
                    break;
                case LayerType.Tree:
                    ApplyTrees(ctx);
                    break;
                case LayerType.Shrub:
                    ApplyShrubs(ctx);
                    break;
                case LayerType.Wildlife:
                    ApplyWildlife(ctx);
                    break;
                case LayerType.OreVein:
                    ApplyOreVeins(ctx);
                    break;
                default:
                    // Fallback: consume RNG to mimic work
                    _ = ctx.Rng.Next();
                    break;
            }
        }

        private static void ApplyCaves(LayerContext ctx)
        {
            if (ctx.Canvas == null) return;
            int caves = ctx.Rng.Next(1, 3);
            for (int i = 0; i < caves; i++)
            {
                int radius = ctx.Rng.Next(2, 5);
                int3 center = new int3(
                    ctx.Rng.Next(radius, Chunk.Size.x - radius),
                    ctx.Rng.Next(radius, Chunk.Size.y - radius),
                    ctx.Rng.Next(radius, Chunk.Height - radius));
                CarveSphere(ctx.Canvas, center, radius);
            }
        }

        private static void ApplyScatter(LayerContext ctx)
        {
            if (ctx.Canvas == null) return;
            int count = ctx.Rng.Next(4, 10);
            for (int i = 0; i < count; i++)
            {
                int2 pos = new int2(
                    ctx.Rng.Next(0, Chunk.Size.x),
                    ctx.Rng.Next(0, Chunk.Size.y));
                int? ground = FindGround(ctx.Canvas.Chunk, pos);
                if (ground == null || ground.Value >= Chunk.Height - 1) continue;
                int3 bpos = new int3(pos.x, pos.y, ground.Value + 1);
                if (!ctx.Canvas.Chunk[bpos.x, bpos.y, bpos.z].IsFilled)
                    ctx.Canvas.SetBlock(bpos, new Block(BlockKind.Rock));
            }
        }

        private static void ApplyWildlife(LayerContext ctx)
        {
            if (ctx.Canvas == null) return;
            int count = ctx.Rng.Next(1, 4);
            for (int i = 0; i < count; i++)
            {
                int2 pos = new int2(
                    ctx.Rng.Next(0, Chunk.Size.x),
                    ctx.Rng.Next(0, Chunk.Size.y));
                int? ground = FindGround(ctx.Canvas.Chunk, pos);
                if (ground == null || ground.Value >= Chunk.Height - 2) continue;
                ctx.Canvas.Spawn(new int3(pos.x, pos.y, ground.Value + 1));
            }
        }

        private static void ApplyOreVeins(LayerContext ctx)
        {
            if (ctx.Canvas == null) return;
            int veins = ctx.Rng.Next(1, 4);
            for (int i = 0; i < veins; i++)
            {
                int3 center = new int3(
                    ctx.Rng.Next(4, Chunk.Size.x - 4),
                    ctx.Rng.Next(4, Chunk.Size.y - 4),
                    ctx.Rng.Next(4, Chunk.Height / 2));
                int radius = ctx.Rng.Next(1, 3);
                FillSphere(ctx.Canvas, center, radius, new Block(BlockKind.GlowingRock));
            }
        }

        private static void CarveSphere(Canvas canvas, int3 center, int radius)
        {
            int r2 = radius * radius;
            for (int x = center.x - radius; x <= center.x + radius; x++)
            for (int y = center.y - radius; y <= center.y + radius; y++)
            for (int z = center.z - radius; z <= center.z + radius; z++)
            {
                if (x < 0 || x >= Chunk.Size.x ||
                    y < 0 || y >= Chunk.Size.y ||
                    z < 0 || z >= Chunk.Height)
                    continue;
                int dx = x - center.x;
                int dy = y - center.y;
                int dz = z - center.z;
                if (dx * dx + dy * dy + dz * dz <= r2)
                    canvas.SetBlock(new int3(x, y, z), Block.Air);
            }
        }

        private static void FillSphere(Canvas canvas, int3 center, int radius, Block block)
        {
            int r2 = radius * radius;
            for (int x = center.x - radius; x <= center.x + radius; x++)
            for (int y = center.y - radius; y <= center.y + radius; y++)
            for (int z = center.z - radius; z <= center.z + radius; z++)
            {
                if (x < 0 || x >= Chunk.Size.x ||
                    y < 0 || y >= Chunk.Size.y ||
                    z < 0 || z >= Chunk.Height)
                    continue;
                int dx = x - center.x;
                int dy = y - center.y;
                int dz = z - center.z;
                if (dx * dx + dy * dy + dz * dz <= r2)
                    canvas.SetBlock(new int3(x, y, z), block);
            }
        }

        private static void ApplyTrees(LayerContext ctx)
        {
            if (ctx.Canvas == null) return;
            int count = ctx.Rng.Next(1, 4);
            for (int i = 0; i < count; i++)
            {
                int2 pos = new int2(
                    ctx.Rng.Next(0, Chunk.Size.x),
                    ctx.Rng.Next(0, Chunk.Size.y));
                BuildSimpleTree(ctx.Canvas, pos, ctx.Rng);
            }
        }

        private static void ApplyShrubs(LayerContext ctx)
        {
            if (ctx.Canvas == null) return;
            int count = ctx.Rng.Next(2, 6);
            for (int i = 0; i < count; i++)
            {
                int2 pos = new int2(
                    ctx.Rng.Next(0, Chunk.Size.x),
                    ctx.Rng.Next(0, Chunk.Size.y));
                BuildShrub(ctx.Canvas, pos);
            }
        }

        private static void BuildSimpleTree(Canvas canvas, int2 local, Random rng)
        {
            int? ground = FindGround(canvas.Chunk, local);
            if (ground == null || ground >= Chunk.Height - 5) return;

            int height = rng.Next(3, 6);
            for (int i = 1; i <= height; i++)
            {
                int z = ground.Value + i;
                canvas.SetBlock(new int3(local.x, local.y, z), new Block(BlockKind.Wood));
            }

            int leafStart = ground.Value + height - 1;
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            for (int dz = 0; dz <= 1; dz++)
            {
                int3 pos = new int3(local.x + dx, local.y + dy, leafStart + dz);
                if (pos.x < 0 || pos.x >= Chunk.Size.x ||
                    pos.y < 0 || pos.y >= Chunk.Size.y ||
                    pos.z < 0 || pos.z >= Chunk.Height)
                    continue;
                if (!canvas.Chunk[pos.x, pos.y, pos.z].IsFilled)
                    canvas.SetBlock(pos, new Block(BlockKind.Leaves));
            }
        }

        private static void BuildShrub(Canvas canvas, int2 local)
        {
            int? ground = FindGround(canvas.Chunk, local);
            if (ground == null || ground >= Chunk.Height - 2) return;
            int z = ground.Value + 1;
            canvas.SetBlock(new int3(local.x, local.y, z), new Block(BlockKind.Leaves));
        }

        private static int? FindGround(Chunk chunk, int2 local)
        {
            for (int z = Chunk.Height - 1; z >= 0; z--)
                if (chunk[local.x, local.y, z].IsFilled)
                    return z;
            return null;
        }
    }
}
