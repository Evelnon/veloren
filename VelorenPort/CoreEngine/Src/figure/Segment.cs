using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.figure
{
    /// <summary>
    /// A volume of <see cref="MatCell"/> used to represent characters or other
    /// figures. This is a simplified version of the Rust implementation and
    /// merely wraps <see cref="Vol{T}"/> with a few helpers.
    /// </summary>
    [Serializable]
    public class Segment : Vol<MatCell>
    {
        public Segment(int3 size) : base(size) { }

        public static Segment Filled(int3 size, MatCell value)
        {
            var seg = new Segment(size);
            seg.Fill(value);
            return seg;
        }
    }
}
