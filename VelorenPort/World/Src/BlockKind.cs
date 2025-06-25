using VelorenPort.CoreEngine;

namespace VelorenPort.World {
    /// <summary>
    /// Enumeración completa de bloques según <c>common/src/terrain/block.rs</c>.
    /// Los valores se mantienen para conservar la lógica original.
    /// </summary>
    public enum BlockKind : byte {
        Air = 0x00,
        Water = 0x01,
        Rock = 0x10,
        WeakRock = 0x11,
        Lava = 0x12,
        GlowingRock = 0x13,
        GlowingWeakRock = 0x14,
        Grass = 0x20,
        Snow = 0x21,
        ArtSnow = 0x22,
        Earth = 0x30,
        Sand = 0x31,
        Wood = 0x40,
        Leaves = 0x41,
        GlowingMushroom = 0x42,
        Ice = 0x43,
        ArtLeaves = 0x44,
        Misc = 0xFE
    }

    public static class BlockKindExtensions {
        public static bool IsAir(this BlockKind kind) => kind == BlockKind.Air;
        public static bool IsFluid(this BlockKind kind) => ((byte)kind & 0xF0) == 0x00;
        public static bool IsLiquid(this BlockKind kind) => kind.IsFluid() && !kind.IsAir();
        public static LiquidKind? LiquidKind(this BlockKind kind) => kind switch {
            BlockKind.Water => CoreEngine.LiquidKind.Water,
            BlockKind.Lava => CoreEngine.LiquidKind.Lava,
            _ => null,
        };
        public static bool IsFilled(this BlockKind kind) => !kind.IsFluid();
        public static bool HasColor(this BlockKind kind) => kind.IsFilled();
        public static bool IsTerrain(this BlockKind kind) =>
            kind == BlockKind.Rock ||
            kind == BlockKind.WeakRock ||
            kind == BlockKind.GlowingRock ||
            kind == BlockKind.GlowingWeakRock ||
            kind == BlockKind.Grass ||
            kind == BlockKind.Earth ||
            kind == BlockKind.Sand;
    }
}
