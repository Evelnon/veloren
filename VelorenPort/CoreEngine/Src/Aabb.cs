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

        /// <summary>
        /// Project the box into screen space using a simple perspective model.
        /// </summary>
        public FindDist.Aabrf ProjectPerspective(float3 viewPos, quaternion viewRot, float fovY, float aspect)
        {
            var invRot = new quaternion(-viewRot.x, -viewRot.y, -viewRot.z, viewRot.w);
            float tanHalf = math.tan(fovY * 0.5f);
            float2 min = new float2(float.PositiveInfinity, float.PositiveInfinity);
            float2 max = new float2(float.NegativeInfinity, float.NegativeInfinity);
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

            foreach (var c in corners)
            {
                float3 p = math.rotate(invRot, (float3)c - viewPos);
                if (p.z <= 0f) continue;
                float2 proj = new float2(p.x / (p.z * tanHalf), p.y / (p.z * tanHalf));
                proj.y /= aspect;
                min = new float2(math.min(min.x, proj.x), math.min(min.y, proj.y));
                max = new float2(math.max(max.x, proj.x), math.max(max.y, proj.y));
            }

            if (!math.isfinite(min.x) || !math.isfinite(min.y))
                return new FindDist.Aabrf(float2.zero, float2.zero);

            return new FindDist.Aabrf(min, max);
        }

        /// <summary>
        /// Perform a swept collision test against <paramref name="other"/>.
        /// </summary>
        public bool SweepTest(Aabb other, int3 delta, out float time)
        {
            float3 vel = (float3)delta;

            if (vel.x == 0 && vel.y == 0 && vel.z == 0)
            {
                time = Overlaps(other) ? 0f : float.PositiveInfinity;
                return Overlaps(other);
            }

            float3 entry;
            float3 exit;

            if (vel.x > 0)
            {
                entry.x = other.Min.x - Max.x;
                exit.x = other.Max.x - Min.x;
            }
            else if (vel.x < 0)
            {
                entry.x = other.Max.x - Min.x;
                exit.x = other.Min.x - Max.x;
            }
            else
            {
                entry.x = float.NegativeInfinity;
                exit.x = float.PositiveInfinity;
            }

            if (vel.y > 0)
            {
                entry.y = other.Min.y - Max.y;
                exit.y = other.Max.y - Min.y;
            }
            else if (vel.y < 0)
            {
                entry.y = other.Max.y - Min.y;
                exit.y = other.Min.y - Max.y;
            }
            else
            {
                entry.y = float.NegativeInfinity;
                exit.y = float.PositiveInfinity;
            }

            if (vel.z > 0)
            {
                entry.z = other.Min.z - Max.z;
                exit.z = other.Max.z - Min.z;
            }
            else if (vel.z < 0)
            {
                entry.z = other.Max.z - Min.z;
                exit.z = other.Min.z - Max.z;
            }
            else
            {
                entry.z = float.NegativeInfinity;
                exit.z = float.PositiveInfinity;
            }

            entry /= vel;
            exit /= vel;

            float entryTime = math.max(math.max(entry.x, entry.y), entry.z);
            float exitTime = math.min(math.min(exit.x, exit.y), exit.z);

            bool hit = entryTime <= exitTime && entryTime >= 0f && entryTime <= 1f;
            time = hit ? entryTime : float.PositiveInfinity;
            return hit;
        }
    }
}
