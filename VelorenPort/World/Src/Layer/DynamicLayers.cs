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
        Wildlife
    }

    /// <summary>
    /// Simplified layer application context.
    /// </summary>
    public class LayerContext
    {
        public int2 ChunkPos { get; init; }
        public Random Rng { get; init; } = new();
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
        /// Apply the requested layer to the chunk. Currently only logs the action.
        /// </summary>
        public static void Apply(LayerType layer, LayerContext ctx)
        {
            switch (layer)
            {
                case LayerType.Scatter:
                    ApplyScatter(ctx);
                    break;
                default:
                    // Placeholder behaviour for other layers
                    _ = ctx.Rng.Next();
                    break;
            }
        }

        private static void ApplyScatter(LayerContext ctx)
        {
            if (ctx.Rng.NextDouble() < ctx.ScatterChance)
            {
                var values = Enum.GetValues(typeof(Spot));
                var spot = (Spot)values.GetValue(ctx.Rng.Next(values.Length))!;
                ctx.Supplement.AddEntity(spot);
            }
        }
    }
}
