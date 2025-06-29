using System;
using Unity.Mathematics;

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
        public Random Rng { get; } = new();
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
            // In the real implementation this would modify the chunk data.
            // For now we simply simulate some work by consuming RNG values.
            _ = ctx.Rng.Next();
            // Future work: integrate generation logic from the Rust project.
        }
    }
}
