using System;

namespace VelorenPort.World {
    /// <summary>
    /// Representa un bloque dentro del terreno. Mantiene la misma
    /// disposición de datos que la estructura original en Rust para
    /// permitir conversiones directas desde y hacia un valor de 32 bits.
    /// </summary>
    [Serializable]
    public struct Block {
        public const float MaxHeight = 3.0f;

        public BlockKind Kind { get; private set; }
        private readonly byte _d0;
        private readonly byte _d1;
        private readonly byte _d2;

        public Block(BlockKind kind, byte d0 = 0, byte d1 = 0, byte d2 = 0) {
            Kind = kind;
            _d0 = d0;
            _d1 = d1;
            _d2 = d2;
        }

        public static Block Filled(BlockKind kind, byte r, byte g, byte b) {
            if (!kind.IsFilled())
                throw new ArgumentException("BlockKind must be solid", nameof(kind));
            return new Block(kind, r, g, b);
        }

        public static Block Air => new Block(BlockKind.Air);

        public bool IsFilled => Kind.IsFilled();

        public byte[] Data => new[] { _d0, _d1, _d2 };

        /// <summary>Convert this block to its raw 32-bit representation.</summary>
        public uint ToUInt32() {
            Span<byte> bytes = stackalloc byte[4];
            bytes[0] = (byte)Kind;
            bytes[1] = _d0;
            bytes[2] = _d1;
            bytes[3] = _d2;
            return BitConverter.ToUInt32(bytes);
        }

        /// <summary>Create a block from its raw 32-bit representation.</summary>
        public static Block FromUInt32(uint value) {
            Span<byte> bytes = stackalloc byte[4];
            BitConverter.TryWriteBytes(bytes, value);
            var kind = (BlockKind)bytes[0];
            return new Block(kind, bytes[1], bytes[2], bytes[3]);
        }

        /// <summary>Retrieve the RGB color if this block stores one.</summary>
        public (byte r, byte g, byte b)? GetColor() {
            return IsFilled ? (_d0, _d1, _d2) : null;
        }

        /// <summary>Return a copy of this block with the data of another block if compatible.</summary>
        public Block WithDataOf(Block other) {
            if (IsFilled == other.IsFilled)
                return new Block(Kind, other._d0, other._d1, other._d2);
            return this;
        }
    }
}
