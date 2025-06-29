using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Simple velocity component mirroring the Rust <c>Vel</c> struct.
    /// </summary>
    [Serializable]
    public struct Vel
    {
        public float3 Value;

        public Vel(float3 value)
        {
            Value = value;
        }

        public static Vel Zero => new Vel(float3.zero);

        /// <summary>Direction of the velocity or zero if stationary.</summary>
        public float3 Direction => math.lengthsq(Value) > 0f ? math.normalize(Value) : float3.zero;

        public static implicit operator float3(Vel v) => v.Value;
        public static implicit operator Vel(float3 v) => new Vel(v);
    }
}
