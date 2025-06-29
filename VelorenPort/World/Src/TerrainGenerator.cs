using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Generación de terreno basada en ruido para calcular la altura de cada bloque.
    /// </summary>
    public static class TerrainGenerator {
        /// <summary>
        /// Genera un chunk compuesto de bloques de tierra hasta una altura determinada por ruido.
        /// </summary>
        public static Chunk GenerateChunk(int2 chunkPos, Noise noise) {
            return GenerateChunkWithSupplement(chunkPos, noise).chunk;
        }

        /// <summary>
        /// Genera un chunk junto con datos suplementarios de recursos y fauna.
        /// </summary>
        public static (Chunk chunk, ChunkSupplement supplement) GenerateChunkWithSupplement(int2 chunkPos, Noise noise) {
            var chunk = new Chunk(chunkPos, Block.Air);
            var supplement = new ChunkSupplement();

            for (int x = 0; x < Chunk.Size.x; x++)
            for (int y = 0; y < Chunk.Size.y; y++) {
                float worldX = chunkPos.x * Chunk.Size.x + x;
                float worldY = chunkPos.y * Chunk.Size.y + y;
                float n = noise.Cave(new float3(worldX * 0.05f, worldY * 0.05f,0));
                int height = (int)math.clamp((n + 1f) * (Chunk.Height / 4f), 0f, Chunk.Height - 1);
                for (int z = 0; z <= height; z++) {
                    BlockKind kind = BlockKind.Earth;
                    if (z < height - 2 && noise.Scatter(new float3(worldX * 0.1f, worldY * 0.1f, z * 0.2f)) > 0.88f)
                        kind = BlockKind.GlowingRock;

                    var block = new Block(kind);
                    chunk[x, y, z] = block;
                }
            }

            var ctx = new Layer.LayerContext { ChunkPos = chunkPos, Noise = noise };
            Layer.LayerManager.Apply(Layer.LayerType.Cave, ctx, chunk);
            Layer.LayerManager.Apply(Layer.LayerType.Scatter, ctx, chunk);
            Layer.LayerManager.Apply(Layer.LayerType.Shrub, ctx, chunk);
            Layer.LayerManager.Apply(Layer.LayerType.Tree, ctx, chunk);
            Layer.LayerManager.Apply(Layer.LayerType.Resource, ctx, chunk);
            Layer.LayerManager.Apply(Layer.LayerType.Wildlife, ctx, chunk);

            foreach (var spawn in chunk.Wildlife)
                supplement.Wildlife.Add(spawn);

            // collect resource blocks after layers have run
            for (int x = 0; x < Chunk.Size.x; x++)
            for (int y = 0; y < Chunk.Size.y; y++)
            for (int z = 0; z < Chunk.Height; z++)
            {
                if (chunk[x, y, z].GetRtsimResource() != null)
                    supplement.ResourceBlocks.Add(new int3(x, y, z));
            }

            return (chunk, supplement);
        }
    }
}
