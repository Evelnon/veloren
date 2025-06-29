using System;

namespace VelorenPort.World.Site {
    /// <summary>
    /// Kind of point of interest with optional associated value.
    /// Mirrors the Rust enum <c>PoiKind</c> used in civilisation code.
    /// </summary>
    [Serializable]
    public struct PoiKind {
        public enum Type { Peak, Biome }
        public Type Kind;
        public uint Value;

        public static PoiKind Peak(uint alt) => new PoiKind { Kind = Type.Peak, Value = alt };
        public static PoiKind Biome(uint size) => new PoiKind { Kind = Type.Biome, Value = size };
        public override string ToString() => Kind switch {
            Type.Peak => $"Peak({Value})",
            Type.Biome => $"Biome({Value})",
            _ => "Unknown"
        };
    }
}
