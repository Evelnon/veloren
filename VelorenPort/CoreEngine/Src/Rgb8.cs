using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Convenience alias for <see cref="Rgb{T}"/> with byte components.
    /// Mirrors the Rgb<u8> usage in the Rust code.
    /// </summary>
    [Serializable]
    public struct Rgb8 {
        public byte R;
        public byte G;
        public byte B;

        public Rgb8(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
        }

        public static implicit operator Rgb<byte>(Rgb8 c) => new Rgb<byte>(c.R, c.G, c.B);
    }
}
