using System;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Simple deterministic permutation RNG similar to RandomPerm in the Rust code.
    /// </summary>
    [Serializable]
    public struct RandomPerm
    {
        public uint Seed { get; private set; }

        public RandomPerm(uint seed)
        {
            Seed = seed;
        }

        public uint Get(uint perm)
        {
            Span<uint> vals = stackalloc uint[2] { Seed, perm };
            return SeedExpan.DiffuseMult(vals);
        }

        public bool Chance(uint perm, float chance)
        {
            return (Get(perm) % (1u << 16)) / (float)(1u << 16) < chance;
        }

        public uint NextU32()
        {
            Seed = Get(Seed) ^ 0xA7535839u;
            return Seed;
        }

        public ulong NextU64()
        {
            uint a = NextU32();
            uint b = NextU32();
            return ((ulong)a << 32) | b;
        }
    }
}
