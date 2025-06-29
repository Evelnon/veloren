using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Simple plane defined by a normal and distance from origin.
    /// Port of util/plane.rs.
    /// </summary>
    [Serializable]
    public struct Plane
    {
        public Dir Normal;
        public float D;

        public Plane(Dir normal)
        {
            Normal = normal;
            D = 0f;
        }

        public float Distance(float3 to) => math.dot(Normal.Value, to) - D;

        public float3 Projection(float3 v) => v - Normal.Value * Distance(v);

        public static Plane XY() => new Plane(new Dir(new float3(0f, 0f, 1f)));
        public static Plane YZ() => new Plane(new Dir(new float3(1f, 0f, 0f)));
        public static Plane ZX() => new Plane(new Dir(new float3(0f, 1f, 0f)));
    }
}
