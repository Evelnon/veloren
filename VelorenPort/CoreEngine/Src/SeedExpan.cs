using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Utilities for deterministic seed diffusion. Mirrors functions in
    /// world/src/util/seed_expan.rs.
    /// </summary>
    public static class SeedExpan
    {
        /// <summary>Simple non-cryptographic diffusion function.</summary>
        public static uint Diffuse(uint a)
        {
            a ^= a.RotateRight(23);
            return a * 2654435761u;
        }

        /// <summary>Diffuse but takes multiple values as input.</summary>
        public static uint DiffuseMult(ReadOnlySpan<uint> values)
        {
            uint state = (1u << 31) - 1u;
            foreach (var v in values)
            {
                state = Diffuse(state ^ v);
            }
            return state;
        }

        /// Helper to rotate bits to the right.
        private static uint RotateRight(this uint value, int bits)
        {
            return (value >> bits) | (value << (32 - bits));
        }
    }
}
