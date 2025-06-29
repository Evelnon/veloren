using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Utility methods for line segments.
    /// Port of common/src/util/lines.rs.
    /// </summary>
    public static class Lines
    {
        public struct LineSegment3
        {
            public float3 Start;
            public float3 End;
            public LineSegment3(float3 start, float3 end)
            {
                Start = start;
                End = end;
            }
        }

        /// <summary>
        /// Compute the closest points between two 3D line segments.
        /// </summary>
        public static (float3, float3) ClosestPoints3D(LineSegment3 n, LineSegment3 m)
        {
            float3 p1 = n.Start;
            float3 p2 = n.End;
            float3 p3 = m.Start;
            float3 p4 = m.End;

            float3 d1 = p2 - p1;
            float3 d2 = p4 - p3;
            float3 d21 = p3 - p1;

            float v22 = math.dot(d2, d2);
            float v11 = math.dot(d1, d1);
            float v21 = math.dot(d2, d1);
            float v21_1 = math.dot(d21, d1);
            float v21_2 = math.dot(d21, d2);

            float denom = v21 * v21 - v22 * v11;
            float s, t;
            if (denom == 0f)
            {
                s = 0f;
                t = (v11 * s - v21_1) / v21;
            }
            else
            {
                s = (v21_2 * v21 - v22 * v21_1) / denom;
                t = (-v21_1 * v21 + v11 * v21_2) / denom;
            }
            s = math.clamp(s, 0f, 1f);
            t = math.clamp(t, 0f, 1f);

            float3 pA = p1 + s * d1;
            float3 pB = p3 + t * d2;
            return (pA, pB);
        }
    }
}
