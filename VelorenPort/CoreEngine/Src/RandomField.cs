using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Deterministic pseudorandom field used for procedural generation.
    /// Mirrors <c>RandomField</c> from the Rust code.
    /// </summary>
    [Serializable]
    public struct RandomField
    {
        public uint Seed { get; }

        public RandomField(uint seed)
        {
            Seed = seed;
        }

        public uint Get(int3 pos)
        {
            uint a = Seed;
            a = (a ^ 61) ^ (a >> 16);
            a += a << 3;
            a ^= (uint)pos.x;
            a ^= a >> 4;
            a *= 0x27d4eb2d;
            a ^= a >> 15;
            a ^= (uint)pos.y;
            a = (a ^ 61) ^ (a >> 16);
            a += a << 3;
            a ^= a >> 4;
            a ^= (uint)pos.z;
            a *= 0x27d4eb2d;
            a ^= a >> 15;
            return a;
        }

        public float GetFloat(int3 pos) => (Get(pos) & 0xFFFF) / (float)(1 << 16);

        public bool Chance(int3 pos, float chance) => GetFloat(pos) < chance;

        public T? Choose<T>(int3 pos, IReadOnlyList<T> items)
        {
            if (items.Count == 0) return default;
            uint value = Get(pos);
            return items[(int)(value % (uint)items.Count)];
        }
    }
}
