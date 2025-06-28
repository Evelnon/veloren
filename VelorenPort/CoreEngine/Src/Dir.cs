using System;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Represents a normalized direction vector.
    /// Ported from common/src/util/dir.rs with simplified features.
    /// </summary>
    [Serializable]
    public struct Dir
    {
        public float3 Value;

        public Dir(float3 v)
        {
            Value = math.normalize(v);
        }

        public static Dir FromUnnormalized(float3 v)
        {
            return new Dir(v);
        }

        public static Dir Up => new Dir(new float3(0f, 0f, 1f));
        public static Dir Down => new Dir(new float3(0f, 0f, -1f));
        public static Dir Left => new Dir(new float3(-1f, 0f, 0f));
        public static Dir Right => new Dir(new float3(1f, 0f, 0f));
        public static Dir Forward => new Dir(new float3(0f, 1f, 0f));
        public static Dir Back => new Dir(new float3(0f, -1f, 0f));

        public static Dir Random2D(System.Random rng)
        {
            float a = (float)rng.NextDouble() * (2f * math.PI);
            return new Dir(new float3(math.cos(a), math.sin(a), 0f));
        }

        public float3 ToFloat3() => Value;

        public static implicit operator float3(Dir d) => d.Value;
    }
}
