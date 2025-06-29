using System;
using VelorenPort.World;
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
        Resource
    }

    /// <summary>
    /// Simplified layer application context.
    /// </summary>
    public class LayerContext
    {
        public int2 ChunkPos { get; init; }
        public Random Rng { get; } = new();
        public Noise Noise { get; init; } = new Noise(0);
        public ChunkSupplement Supplement { get; init; } = new();
        /// <summary>
        /// Probability that <see cref="LayerType.Scatter"/> will add a spot.
        /// Mainly used in tests to force deterministic output.
        /// </summary>
        public double ScatterChance { get; init; } = 0.1;
    }

    /// <summary>
    /// Manager responsible for applying layers to world chunks.
    /// </summary>
    public static class LayerManager
    {
        /// <summary>
        /// Apply the requested layer to the chunk. This now contains
        /// small scale implementations for caves and scatter objects.
        /// Other layers remain as placeholders.
        /// </summary>
        public static void Apply(LayerType layer, LayerContext ctx, Chunk chunk)
        {
            switch (layer)
            {
                case LayerType.Cave:
                    ApplyCave(ctx, chunk);
                    break;
                case LayerType.Scatter:
                    ApplyScatter(ctx, chunk);
                    break;
                case LayerType.Shrub:
                    ApplyShrub(ctx, chunk);
                    break;
                case LayerType.Tree:
                    ApplyTree(ctx, chunk);
                    break;
                case LayerType.Wildlife:
                    ApplyWildlife(ctx, chunk);
                    break;
                case LayerType.Resource:
                    ApplyResource(ctx, chunk);
                    break;
                default:
                    _ = ctx.Rng.Next();
                    break;
            }
        }

        private static void ApplyCave(LayerContext ctx, Chunk chunk)
        {
            for (int x = 0; x < Chunk.Size.x; x++)
            for (int y = 0; y < Chunk.Size.y; y++)
            for (int z = 1; z < Chunk.Height / 2; z++)
            {
                float3 wpos = new float3(
                    chunk.Position.x * Chunk.Size.x + x,
                    chunk.Position.y * Chunk.Size.y + y,
                    z);
                float n = ctx.Noise.Cave(wpos * 0.08f);
                if (n > 0.6f)
                    chunk[x, y, z] = Block.Air;
            }
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
                    // simple 3-block trunk with a cross of leaves
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

        private static void ApplyWildlife(LayerContext ctx, Chunk chunk)
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
                float n = ctx.Noise.Wildlife(wpos * 0.15f);
                if (n > 0.85f)
                {
                    FaunaKind kind = (FaunaKind)((int)(n * 10) % 4 + 1);
                    chunk.AddWildlife(new FaunaSpawn((int3)wpos, kind));
                }
            }
        }

        private static void ApplyResource(LayerContext ctx, Chunk chunk)
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
        private static void ApplyScatter(LayerContext ctx, Chunk chunk)
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
                    top);
                float n = ctx.Noise.Scatter(wpos * 0.1f);
                if (n > 0.75f)
                    chunk[x, y, top + 1] = new Block(BlockKind.Wood);
            }
        }
    }
}
