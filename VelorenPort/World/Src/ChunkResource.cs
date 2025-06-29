using System;
using System.Collections.Generic;

namespace VelorenPort.World {
    /// <summary>
    /// Resources that can spawn within a chunk. Mirrors <c>ChunkResource</c>
    /// from <c>rtsim.rs</c>.
    /// </summary>
    [Serializable]
    public enum ChunkResource {
        Grass,
        Flower,
        Fruit,
        Vegetable,
        Mushroom,
        Loot,
        Plant,
        Stone,
        Wood,
        Gem,
        Ore
    }

    /// <summary>
    /// Helper utilities for working with <see cref="ChunkResource"/> values.
    /// These defaults can be used by AI and pathfinding systems when no
    /// specific cost configuration is supplied.
    /// </summary>
    public static class ChunkResourceUtil {
        /// <summary>Default weighting when resources influence path cost.</summary>
        public static readonly System.Collections.Generic.Dictionary<ChunkResource, float> DefaultWeights = new()
        {
            [ChunkResource.Grass] = 1f,
            [ChunkResource.Flower] = 0.5f,
            [ChunkResource.Fruit] = -1f,
            [ChunkResource.Vegetable] = -1f,
            [ChunkResource.Mushroom] = -0.5f,
            [ChunkResource.Loot] = -5f,
            [ChunkResource.Plant] = 0.5f,
            [ChunkResource.Stone] = 2f,
            [ChunkResource.Wood] = -0.5f,
            [ChunkResource.Gem] = -4f,
            [ChunkResource.Ore] = -2f,
        };
    }
}
