using System;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Represents a vertical column of blocks at a specific world position.
    /// Provides convenient indexed access similar to the Rust implementation.
    /// </summary>
    [Serializable]
    public class Column {
        public int2 Position { get; }
        private readonly Block[] _blocks = new Block[Chunk.Height];

        public Column(int2 position, Block fill) {
            Position = position;
            for (int i = 0; i < _blocks.Length; i++) _blocks[i] = fill;
        }

        public Block this[int z] {
            get => _blocks[z];
            set => _blocks[z] = value;
        }
    }
}
