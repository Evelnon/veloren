using System;

namespace VelorenPort.CoreEngine {
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
}
