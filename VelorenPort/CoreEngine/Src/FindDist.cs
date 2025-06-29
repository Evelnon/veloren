using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Geometry helpers to approximate range checks and compute minimum
    /// distances between simple shapes. Ported from common/src/util/find_dist.rs.
    /// </summary>
    public static class FindDist
    {
        /// <summary>Axis-aligned bounding box in 3D.</summary>
        public struct Aabb
        {
            public float3 Min;
            public float3 Max;
            public Aabb(float3 min, float3 max) { Min = min; Max = max; }

            public bool ContainsPoint(float3 p) =>
                p.x >= Min.x && p.y >= Min.y && p.z >= Min.z &&
                p.x <= Max.x && p.y <= Max.y && p.z <= Max.z;

            public bool CollidesWith(Aabb other) =>
                Min.x <= other.Max.x && Max.x >= other.Min.x &&
                Min.y <= other.Max.y && Max.y >= other.Min.y &&
                Min.z <= other.Max.z && Max.z >= other.Min.z;
        }

        /// <summary>Axis-aligned bounding rectangle in 2D with float coordinates.</summary>
        public struct Aabrf
        {
            public float2 Min;
            public float2 Max;
            public Aabrf(float2 min, float2 max) { Min = min; Max = max; }
            public float DistanceToPoint(float2 p)
            {
                float2 clamped = math.clamp(p, Min, Max);
                return math.distance(p, clamped);
            }
        }

        public struct Cylinder
        {
            public float3 Center;
            public float Radius;
            public float Height;
            public Cylinder(float3 center, float radius, float height)
            {
                Center = center; Radius = radius; Height = height;
            }
            public Aabb ToAabb() => new(Center - new float3(Radius, Radius, Height * 0.5f),
                                         Center + new float3(Radius, Radius, Height * 0.5f));
        }

        public struct Cube
        {
            public float3 Min;
            public float SideLength;
            public Cube(float3 min, float sideLength)
            {
                Min = min; SideLength = sideLength;
            }
        }

        public static bool ApproxInRange(Cube cube, Cylinder cyl, float range)
        {
            var cubeBox = new Aabb(cube.Min - new float3(range),
                                     cube.Min + new float3(cube.SideLength + range));
            return cubeBox.CollidesWith(cyl.ToAabb());
        }

        public static float MinDistance(Cube cube, Cylinder cyl)
        {
            float zCenter = math.abs(cube.Min.z + cube.SideLength * 0.5f - cyl.Center.z);
            float zDist = math.max(zCenter - (cube.SideLength + cyl.Height) * 0.5f, 0f);
            var square = new Aabrf(cube.Min.xy, cube.Min.xy + cube.SideLength);
            float xyDist = math.max(square.DistanceToPoint(cyl.Center.xy) - cyl.Radius, 0f);
            return math.sqrt(zDist * zDist + xyDist * xyDist);
        }

        public static bool ApproxInRange(Cylinder cyl, Cube cube, float range) =>
            ApproxInRange(cube, cyl, range);

        public static float MinDistance(Cylinder cyl, Cube cube) =>
            MinDistance(cube, cyl);

        public static bool ApproxInRange(Cylinder a, Cylinder b, float range)
        {
            var box = a.ToAabb();
            box.Min -= new float3(range);
            box.Max += new float3(range);
            return box.CollidesWith(b.ToAabb());
        }

        public static float MinDistance(Cylinder a, Cylinder b)
        {
            float zCenter = math.abs(a.Center.z - b.Center.z);
            float zDist = math.max(zCenter - (a.Height + b.Height) * 0.5f, 0f);
            float xyDist = math.max(math.distance(a.Center.xy, b.Center.xy) - a.Radius - b.Radius, 0f);
            return math.sqrt(zDist * zDist + xyDist * xyDist);
        }

        public static bool ApproxInRange(Cylinder cyl, float3 point, float range)
        {
            var box = cyl.ToAabb();
            box.Min -= new float3(range);
            box.Max += new float3(range);
            return box.ContainsPoint(point);
        }

        public static float MinDistance(Cylinder cyl, float3 point)
        {
            float zCenter = math.abs(cyl.Center.z - point.z);
            float zDist = math.max(zCenter - cyl.Height * 0.5f, 0f);
            float xyDist = math.max(math.distance(cyl.Center.xy, point.xy) - cyl.Radius, 0f);
            return math.sqrt(zDist * zDist + xyDist * xyDist);
        }

        public static bool ApproxInRange(float3 point, Cylinder cyl, float range) =>
            ApproxInRange(cyl, point, range);

        public static float MinDistance(float3 point, Cylinder cyl) =>
            MinDistance(cyl, point);
    }
}
