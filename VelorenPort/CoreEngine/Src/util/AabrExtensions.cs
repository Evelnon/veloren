using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Extension methods for <see cref="Aabr"/> providing union, intersection
    /// and point containment operations.
    /// </summary>
    public static class AabrExtensions
    {
        /// <summary>Check if <paramref name="point"/> lies within <paramref name="aabr"/>.</summary>
        public static bool ContainsPoint(this Aabr aabr, int2 point)
            => point.x >= aabr.Min.x && point.y >= aabr.Min.y &&
               point.x < aabr.Max.x && point.y < aabr.Max.y;

        /// <summary>Return a box encompassing <paramref name="aabr"/> and <paramref name="other"/>.</summary>
        public static Aabr Union(this Aabr aabr, Aabr other)
        {
            var min = new int2(Math.Min(aabr.Min.x, other.Min.x), Math.Min(aabr.Min.y, other.Min.y));
            var max = new int2(Math.Max(aabr.Max.x, other.Max.x), Math.Max(aabr.Max.y, other.Max.y));
            return new Aabr(min, max);
        }

        /// <summary>Return the overlapping region of <paramref name="aabr"/> and <paramref name="other"/>.</summary>
        public static Aabr Intersection(this Aabr aabr, Aabr other)
        {
            var min = new int2(Math.Max(aabr.Min.x, other.Min.x), Math.Max(aabr.Min.y, other.Min.y));
            var max = new int2(Math.Min(aabr.Max.x, other.Max.x), Math.Min(aabr.Max.y, other.Max.y));
            return new Aabr(min, max);
        }
    }
}
