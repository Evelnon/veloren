using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Constants and helpers related to terrain chunk sizes.
    /// </summary>
    public static class TerrainConstants
    {
        /// <summary>
        /// Base-two logarithm of the number of blocks along each side of a chunk.
        /// Matches <c>TERRAIN_CHUNK_BLOCKS_LG</c> in the original project.
        /// </summary>
        public const int ChunkBlocksLg = 5;

        /// <summary>
        /// Size of a chunk in blocks.
        /// </summary>
        public static readonly int2 ChunkSize = new int2(1 << ChunkBlocksLg, 1 << ChunkBlocksLg);

        /// <summary>
        /// Convert world block coordinates to chunk coordinates.
        /// </summary>
        public static int2 WorldToChunk(int2 world)
        {
            return new int2(world.x >> ChunkBlocksLg, world.y >> ChunkBlocksLg);
        }
    }
}
