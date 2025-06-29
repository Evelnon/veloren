using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Maintains a collection of generated <see cref="Chunk"/> instances.
    /// New chunks are created on demand using <see cref="TerrainGenerator"/>.
    /// </summary>
    public class WorldMap {
        private readonly Dictionary<int2, Chunk> _chunks = new();
        private readonly Dictionary<int2, ChunkSupplement> _supplements = new();

        /// <summary>
        /// Retrieve a chunk at the given position. If the chunk is not
        /// loaded a new one is generated using the provided noise instance.
        /// </summary>
        public Chunk GetOrGenerate(int2 chunkPos, Noise noise) {
            return GetOrGenerateWithSupplement(chunkPos, noise).chunk;
        }

        /// <summary>
        /// Retrieve a chunk along with its supplement data. The data is cached
        /// so subsequent calls return the same objects.
        /// </summary>
        public (Chunk chunk, ChunkSupplement supplement) GetOrGenerateWithSupplement(int2 chunkPos, Noise noise) {
            if (!_chunks.TryGetValue(chunkPos, out var chunk)) {
                var res = TerrainGenerator.GenerateChunkWithSupplement(chunkPos, noise);
                _chunks[chunkPos] = res.chunk;
                _supplements[chunkPos] = res.supplement;
                return res;
            }

            if (!_supplements.TryGetValue(chunkPos, out var sup)) {
                sup = new ChunkSupplement();
                _supplements[chunkPos] = sup;
            }
            return (chunk, sup);
        }

        /// <summary>Get supplement data for an already generated chunk.</summary>
        public ChunkSupplement? GetSupplement(int2 chunkPos) {
            _supplements.TryGetValue(chunkPos, out var sup);
            return sup;
        }

        /// <summary>
        /// Enumerate all currently loaded chunks.
        /// </summary>
        public IEnumerable<Chunk> Chunks => _chunks.Values;
    }
}
