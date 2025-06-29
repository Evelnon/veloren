using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Axis-aligned bounding box of integer coordinates.
    /// </summary>
    [Serializable]
    public struct Aabb
    {
        public int3 Min;
        public int3 Max;

        public Aabb(int3 min, int3 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Size of the box in each dimension.
        /// </summary>
        public int3 Size => Max - Min;

        /// <summary>
        /// True if the point lies within the box bounds (inclusive of Min and exclusive of Max).
        /// </summary>
        public bool Contains(int3 pos) =>
            pos.x >= Min.x && pos.y >= Min.y && pos.z >= Min.z &&
            pos.x < Max.x && pos.y < Max.y && pos.z < Max.z;

        /// <summary>
        /// Check if two boxes overlap.
        /// </summary>
        public bool Overlaps(Aabb other) =>
            Min.x < other.Max.x && Max.x > other.Min.x &&
            Min.y < other.Max.y && Max.y > other.Min.y &&
            Min.z < other.Max.z && Max.z > other.Min.z;

        /// <summary>
        /// Expand the box equally in all directions.
        /// </summary>
        public Aabb Grow(int3 by) => new Aabb(Min - by, Max + by);

        /// <summary>
        /// Clamp a position so it lies inside the box.
        /// </summary>
        public int3 Clamp(int3 pos) => math.clamp(pos, Min, Max);

        /// <summary>
        /// Combine with <paramref name="other"/> to produce a box that
        /// encompasses both.
        /// </summary>
        public Aabb Union(Aabb other) =>
            new Aabb(math.min(Min, other.Min), math.max(Max, other.Max));

        /// <summary>
        /// Return a new box translated by the given offset.
        /// </summary>
        public Aabb Translate(int3 offset) => new Aabb(Min + offset, Max + offset);
    }
}
