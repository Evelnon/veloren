using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Editable view of a terrain chunk with access to column data.
    /// This is a greatly simplified version of the original Rust Canvas.
    /// </summary>
    [Serializable]
    public struct CanvasInfo {
        public int2 ChunkPos;
        public int2 Wpos;
        public ColumnGen Gen;
        public SimChunk SimChunk;
        public WorldSim Sim;

        public CanvasInfo(int2 chunkPos, WorldSim sim, SimChunk simChunk) {
            ChunkPos = chunkPos;
            Sim = sim;
            SimChunk = simChunk;
            Gen = new ColumnGen(sim);
            Wpos = TerrainChunkSize.CposToWpos(chunkPos);
        }
    }

    /// <summary>
    /// Provides read and write access to chunk blocks and iterates over columns.
    /// </summary>
    [Serializable]
    public class Canvas {
        private readonly CanvasInfo _info;
        private readonly Chunk _chunk;
        private readonly List<int3> _resourceBlocks = new();

        public Canvas(CanvasInfo info, Chunk chunk) {
            _info = info;
            _chunk = chunk;
        }

        public CanvasInfo Info => _info;
        public Chunk Chunk => _chunk;

        public Block GetBlock(int3 pos) => _chunk[pos.x, pos.y, pos.z];

        public void SetBlock(int3 pos, Block block) {
            _chunk[pos.x, pos.y, pos.z] = block;
        }

        /// <summary>
        /// Enumerate all columns within this canvas and invoke <paramref name="action"/> with the sampled data.
        /// </summary>
        public void ForEachColumn(Action<int2, ColumnSample> action) {
            int2 size = TerrainChunkSize.RectSize;
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++) {
                int2 wpos = _info.Wpos + new int2(x, y);
                var sample = _info.Gen.Get((wpos, (object)null!, (object?)null));
                if (sample != null) {
                    action(wpos, sample);
                }
            }
        }

        /// <summary>
        /// Retrieve a <see cref="Column"/> representing blocks at the given
        /// world position.
        /// </summary>
        public Column GetColumn(int2 wpos) {
            int2 local = wpos - _info.Wpos;
            var column = new Column(wpos, Block.Air);
            for (int z = 0; z < Chunk.Height; z++) {
                column[z] = _chunk[local.x, local.y, z];
            }
            return column;
        }
    }
}
