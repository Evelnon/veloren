using System;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Sim {
    /// <summary>
    /// Representation of road connections between chunks.
    /// Mirrors <c>way.rs</c>.
    /// </summary>
    [Serializable]
    public struct Way {
        public int2 Offset;
        public byte Neighbors;

        public bool IsWay => Neighbors != 0;
        public void Clear() => Neighbors = 0;
    }

    [Serializable]
    public struct Path {
        public float Width;
        public static Path Default => new Path { Width = 5f };

        /// <summary>
        /// Interpolate two paths.
        /// </summary>
        public static Path Lerp(Path from, Path to, float factor)
        {
            return new Path { Width = math.lerp(from.Width, to.Width, factor) };
        }

        /// <summary>
        /// Return the number of blocks of head space required at the given
        /// distance from the path centre.
        /// </summary>
        public int HeadSpace(float dist)
        {
            return math.max(1, (int)math.round(8f - math.pow(dist * 0.25f, 6))); 
        }

        /// <summary>Get the surface colour of a path given the surrounding colour.</summary>
        public Rgb8 SurfaceColor(Rgb8 col, int3 wpos)
        {
            Rgb8 scaled = new Rgb8(
                (byte)math.round(col.R * 0.7f),
                (byte)math.round(col.G * 0.7f),
                (byte)math.round(col.B * 0.7f)
            );
            return NoisyColor(scaled, 8, wpos);
        }

        /// <summary>Add deterministic noise to a colour.</summary>
        public static Rgb8 NoisyColor(Rgb8 color, uint factor, int3 wpos)
        {
            uint nz = new RandomField(0).Get(wpos);
            byte Apply(byte c)
            {
                int val = c + (int)(nz % (factor * 2)) - (int)factor;
                return (byte)math.clamp(val, 0, 255);
            }
            return new Rgb8(Apply(color.R), Apply(color.G), Apply(color.B));
        }
    }
}
