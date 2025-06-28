using System;
using Unity.Mathematics;

namespace VelorenPort.World
{
    /// <summary>
    /// Base-two logarithm of a world size, in chunks per dimension.
    /// Simplified representation of the Rust struct.
    /// </summary>
    [Serializable]
    public struct MapSizeLg
    {
        public int2 Value;

        public static readonly int2 MaxWorldBlocksLg = new int2(19, 19);

        public MapSizeLg(int2 value)
        {
            if (value.x < 0 || value.y < 0 ||
                value.x > MaxWorldBlocksLg.x - TerrainChunkSize.TERRAIN_CHUNK_BLOCKS_LG ||
                value.y > MaxWorldBlocksLg.y - TerrainChunkSize.TERRAIN_CHUNK_BLOCKS_LG)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            Value = value;
        }

        public int2 Chunks => new int2(1 << Value.x, 1 << Value.y);

        public int ChunksLen => 1 << (Value.x + Value.y);

        public bool ContainsChunk(int2 chunkPos)
        {
            int2 size = Chunks;
            return chunkPos.x >= 0 && chunkPos.y >= 0 &&
                   (chunkPos.x & (size.x - 1)) == chunkPos.x &&
                   (chunkPos.y & (size.y - 1)) == chunkPos.y;
        }

        public int2 UniformIdxAsVec2(int idx)
        {
            int xMask = (1 << Value.x) - 1;
            return new int2(idx & xMask, idx >> Value.x);
        }

        public int Vec2AsUniformIdx(int2 pos)
        {
            return (pos.y << Value.x) | (pos.x & ((1 << Value.x) - 1));
        }
    }
}
