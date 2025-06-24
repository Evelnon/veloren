using System;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Simple position wrapper matching the Pos component in Rust.
    /// </summary>
    [Serializable]
    public struct Pos {
        public float3 Value;
        public Pos(float3 value) { Value = value; }

        public static implicit operator float3(Pos p) => p.Value;
        public static implicit operator Pos(float3 v) => new Pos(v);
        public override string ToString() => Value.ToString();
    }
}
