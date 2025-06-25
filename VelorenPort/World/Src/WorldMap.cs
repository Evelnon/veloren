using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Maintains a collection of generated <see cref="Chunk"/> instances.
    /// New chunks are created on demand using <see cref="TerrainGenerator"/>.
    /// </summary>
    public class WorldMap {
        private readonly Dictionary<int2, Chunk> _chunks = new();

        /// <summary>
        /// Retrieve a chunk at the given position. If the chunk is not
        /// loaded a new one is generated using the provided noise instance.
        /// </summary>
        public Chunk GetOrGenerate(int2 chunkPos, Noise noise) {
            if (!_chunks.TryGetValue(chunkPos, out var chunk)) {
                chunk = TerrainGenerator.GenerateChunk(chunkPos, noise);
                _chunks[chunkPos] = chunk;
            }
            return chunk;
        }

        /// <summary>
        /// Enumerate all currently loaded chunks.
        /// </summary>
        public IEnumerable<Chunk> Chunks => _chunks.Values;
    }
}
