using System;

namespace VelorenPort.World
{
    /// <summary>
    /// Enumeration of all possible biomes. Mirrors <c>biome.rs</c>.
    /// </summary>
    [Serializable]
    public enum BiomeKind
    {
        Void,
        Lake,
        Grassland,
        Ocean,
        Mountain,
        Snowland,
        Desert,
        Swamp,
        Jungle,
        Forest,
        Savannah,
        Taiga,
    }

    public static class BiomeKindExtensions
    {
        /// <summary>
        /// Rough difficulty rating of the biome, 1 to 5.
        /// </summary>
        public static int Difficulty(this BiomeKind kind)
        {
            return kind switch
            {
                BiomeKind.Void => 1,
                BiomeKind.Lake => 1,
                BiomeKind.Grassland => 2,
                BiomeKind.Ocean => 1,
                BiomeKind.Mountain => 1,
                BiomeKind.Snowland => 2,
                BiomeKind.Desert => 5,
                BiomeKind.Swamp => 2,
                BiomeKind.Jungle => 3,
                BiomeKind.Forest => 1,
                BiomeKind.Savannah => 2,
                BiomeKind.Taiga => 2,
                _ => 1,
            };
        }
    }
}
