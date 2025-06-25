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
            var chunk = new Chunk(chunkPos, Block.Air);
            for (int x = 0; x < Chunk.Size.x; x++)
            for (int y = 0; y < Chunk.Size.y; y++) {
                float worldX = chunkPos.x * Chunk.Size.x + x;
                float worldY = chunkPos.y * Chunk.Size.y + y;
                float n = noise.Cave(new float3(worldX * 0.05f, worldY * 0.05f, 0));
                int height = (int)math.clamp((n + 1f) * (Chunk.Height / 4f), 0f, Chunk.Height - 1);
                for (int z = 0; z <= height; z++)
                    chunk[x, y, z] = new Block(BlockKind.Earth);
            }
            return chunk;
        }
    }
}
