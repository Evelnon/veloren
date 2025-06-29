using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Simple orientation component mirroring the Ori struct from Rust.
    /// Stores a normalized quaternion.
    /// </summary>
    [Serializable]
    public struct Ori
    {
        public quaternion Value;
        public Ori(quaternion value)
        {
            Value = math.normalize(value);
        }

        public static Ori Identity => new Ori(quaternion.identity);
        public static implicit operator quaternion(Ori o) => o.Value;
        public static implicit operator Ori(quaternion q) => new Ori(q);
    }
}
