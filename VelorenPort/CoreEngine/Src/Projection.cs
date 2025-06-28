using Unity.Mathematics;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Extensions to project vectors onto other vectors or planes.
    /// Mirrors util/projection.rs from the Rust code.
    /// </summary>
    public static class Projection
    {
        public static float2 Projected(this float2 v, float2 onto)
        {
            float denom = math.lengthsq(onto);
            return denom == 0f ? float2.zero : math.dot(v, onto) * onto / denom;
        }

        public static float3 Projected(this float3 v, float3 onto)
        {
            float denom = math.lengthsq(onto);
            return denom == 0f ? float3.zero : math.dot(v, onto) * onto / denom;
        }

        public static float3 Projected(this float3 v, Dir onto) => v.Projected(onto.Value);

        public static float3 Projected(this float3 v, Plane plane) => plane.Projection(v);
    }
}
