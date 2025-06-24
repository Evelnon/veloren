using System;

namespace VelorenPort.World {
    /// <summary>
    /// Representa un bloque simple dentro del terreno.
    /// </summary>
    [Serializable]
    public struct Block {
        public BlockKind Kind { get; private set; }

        public Block(BlockKind kind) {
            Kind = kind;
        }

        public static Block Air => new Block(BlockKind.Air);
    }
}
