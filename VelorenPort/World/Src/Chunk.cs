using System;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Simple terrain chunk storing blocks in a fixed size 3D grid.
    /// </summary>
    [Serializable]
    public class Chunk {
        public static readonly int2 Size = new int2(1 << 5, 1 << 5);
        public const int Height = 32;

        private readonly Block[,,] _blocks = new Block[Size.x, Size.y, Height];

        public int2 Position { get; }

        public Chunk(int2 position, Block fill) {
            Position = position;
            for (int z = 0; z < Height; z++)
            for (int y = 0; y < Size.y; y++)
            for (int x = 0; x < Size.x; x++)
                _blocks[x, y, z] = fill;
        }

        public Block this[int x, int y, int z] {
            get => _blocks[x, y, z];
            set => _blocks[x, y, z] = value;
        }
    }
}
