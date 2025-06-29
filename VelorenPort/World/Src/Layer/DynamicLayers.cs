using System;
using VelorenPort.World;
using VelorenPort.NativeMath;

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
        Spot,
        Rock,
        Vegetation,
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
        /// Probability that <see cref="LayerType.Spot"/> will add a spot.
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
        /// Apply the requested layer to the given chunk. Some layers
        /// only modify supplementary data and ignore the chunk reference.
        /// </summary>
        public static void Apply(LayerType layer, LayerContext ctx, Chunk? chunk = null)
        {
            switch (layer)
            {
                case LayerType.Cave:
                    if (chunk != null) CaveLayer.Apply(ctx, chunk);
                    break;
                case LayerType.Spot:
                    SpotLayer.Apply(ctx);
                    break;
                case LayerType.Rock:
                    if (chunk != null) RockLayer.Apply(ctx, chunk);
                    break;
                case LayerType.Vegetation:
                    if (chunk != null) VegetationLayer.Apply(ctx, chunk);
                    break;
                case LayerType.Wildlife:
                    if (chunk != null) WildlifeLayer.Apply(ctx, chunk);
                    break;
                case LayerType.Resource:
                    if (chunk != null) ResourceLayer.Apply(ctx, chunk);
                    break;
                default:
                    _ = ctx.Rng.Next();
                    break;
            }
        }

    }
}
