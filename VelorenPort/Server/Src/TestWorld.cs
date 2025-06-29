using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.World;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server {
    /// <summary>
    /// Minimal test world used by the server when no real world
    /// generation is available. Mirrors <c>server/src/test_world.rs</c>.
    /// </summary>
    public class TestWorld {
        private static readonly MapSizeLg DEFAULT_WORLD_CHUNKS_LG =
            new MapSizeLg(new int2(8, 8));

        public class IndexOwned {
            public IndexRef AsIndexRef() => new IndexRef(this);
            public T? ReloadIfChanged<T>(Func<IndexOwned, T> reload) => null;
        }

        public readonly struct IndexRef {
            private readonly IndexOwned _index;
            public IndexRef(IndexOwned index) { _index = index; }
        }

        public static (TestWorld world, IndexOwned index) Generate(uint seed) {
            return (new TestWorld(), new IndexOwned());
        }

        public void Tick(TimeSpan dt) { }

        public MapSizeLg MapSizeLg() => DEFAULT_WORLD_CHUNKS_LG;

        public Chunk GenerateOobChunk() => new Chunk(int2.zero, Block.Air);

        public (Chunk chunk, ChunkSupplement supplement) GenerateChunk(
            IndexRef index,
            int2 chunkPos,
            Dictionary<ChunkResource, float>? rtsimResources,
            Func<bool> shouldContinue,
            (TimeOfDay, Calendar)? time)
        {
            var seed = (uint)(chunkPos.x * 31 + chunkPos.y * 17);
            var rng = new VelorenPort.NativeMath.Random(seed + 1u);
            int height = (int)(rng.NextUInt() % 8);
            var chunk = new Chunk(chunkPos, Block.Air);
            int baseZ = rng.NextUInt(0, 256) < 64 ? height : 0;
            bool night = time.HasValue && time.Value.Item1.Value % (24*3600) > 43200;
            var kind = night ? BlockKind.Snow : BlockKind.Grass;
            for (int z = 0; z < baseZ; z++)
                for (int y = 0; y < Chunk.Size.y; y++)
                    for (int x = 0; x < Chunk.Size.x; x++)
                        chunk[x, y, z] = Block.Filled(kind, 11, 102, 35);
            return (chunk, new ChunkSupplement());
        }

        public int2 GetCenter() {
            int2 chunks = DEFAULT_WORLD_CHUNKS_LG.Chunks;
            return chunks / 2 * TerrainChunkSize.RectSize.x;
        }

        public string? GetLocationName(IndexRef index, int2 wpos2d) => null;
    }
}
