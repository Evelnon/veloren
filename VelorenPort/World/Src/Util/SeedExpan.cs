using System;

namespace VelorenPort.World.Util
{
    /// <summary>
    /// Deterministic seed expansion utilities mirrored from the Rust project.
    /// </summary>
    public static class SeedExpan
    {
        /// <summary>Simple non-cryptographic diffusion function.</summary>
        public static uint Diffuse(uint a)
        {
            a ^= RotateRight(a, 23);
            return a * 2654435761u;
        }

        /// <summary>Diffuse multiple values.</summary>
        public static uint DiffuseMult(ReadOnlySpan<uint> values)
        {
            uint state = (1u << 31) - 1u;
            foreach (var v in values)
                state = Diffuse(state ^ v);
            return state;
        }

        /// <summary>
        /// Expand a 32-bit seed into a 32 byte RNG state.
        /// </summary>
        public static byte[] RngState(uint x)
        {
            Span<uint> tmp = stackalloc uint[8];
            for (int i = 0; i < 8; i++)
            {
                x = Diffuse(x);
                tmp[i] = x;
            }
            byte[] bytes = new byte[32];
            Buffer.BlockCopy(tmp.ToArray(), 0, bytes, 0, 32);
            return bytes;
        }

        private static uint RotateRight(uint value, int bits)
        {
            return (value >> bits) | (value << (32 - bits));
        }
    }
}
