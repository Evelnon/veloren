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

        /// <summary>
        /// Return the overlapping region between this box and <paramref name="other"/>.
        /// </summary>
        public Aabb Intersection(Aabb other) =>
            new Aabb(math.max(Min, other.Min), math.min(Max, other.Max));

        /// <summary>
        /// Multiply the bounds of the box by a uniform scale factor.
        /// </summary>
        public Aabb Scale(int factor) => Scale(new int3(factor, factor, factor));

        /// <summary>
        /// Multiply the bounds of the box by a non-uniform scale factor.
        /// </summary>
        public Aabb Scale(int3 factor) => new Aabb(Min * factor, Max * factor);

        /// <summary>
        /// Rotate the box by <paramref name="rotation"/> around the origin and
        /// return a new axis-aligned box that contains the result.
        /// </summary>
        public Aabb Rotate(quaternion rotation)
        {
            var corners = new int3[8]
            {
                new int3(Min.x, Min.y, Min.z),
                new int3(Max.x, Min.y, Min.z),
                new int3(Min.x, Max.y, Min.z),
                new int3(Max.x, Max.y, Min.z),
                new int3(Min.x, Min.y, Max.z),
                new int3(Max.x, Min.y, Max.z),
                new int3(Min.x, Max.y, Max.z),
                new int3(Max.x, Max.y, Max.z)
            };

            int3 rotMin = (int3)math.round(math.rotate(rotation, (float3)corners[0]));
            int3 rotMax = rotMin;
            for (int i = 1; i < corners.Length; i++)
            {
                int3 r = (int3)math.round(math.rotate(rotation, (float3)corners[i]));
                rotMin = math.min(rotMin, r);
                rotMax = math.max(rotMax, r);
            }
            return new Aabb(rotMin, rotMax);
        }
    }
}
