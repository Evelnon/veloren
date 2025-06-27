using System;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Metadata about a region used by <see cref="WorldSim"/>. This is a
    /// lightweight counterpart of the Rust structure of the same name.
    /// </summary>
    [Serializable]
    public struct RegionInfo {
        public int2 ChunkPos;
        public int2 BlockPos;
        public float Dist;
        public uint Seed;

        public RegionInfo(int2 chunkPos, int2 blockPos, float dist, uint seed) {
            ChunkPos = chunkPos;
            BlockPos = blockPos;
            Dist = dist;
            Seed = seed;
        }
    }
}
