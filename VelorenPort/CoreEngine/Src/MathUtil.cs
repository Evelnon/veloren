using Unity.Mathematics;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Utility math helpers used across the port.
    /// Mirrors functionality from util/math.rs.
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        /// Value between 0 and 1 representing how close <paramref name="x"/> is to the
        /// centre of the range. Implements the bell-like curve from the original Rust code.
        /// </summary>
        public static float Close(float x, (float start, float end) range)
        {
            float mean = (range.start + range.end) * 0.5f;
            float width = (range.end - range.start) * 0.5f;
            float t = math.clamp((x - mean) / width, -1f, 1f);
            float r = 1f - t * t;
            return r * r;
        }

        /// <summary>
        /// Create a quaternion that rotates <paramref name="forward"/> toward the given
        /// direction. Simplified version of Unity's <c>Quaternion.LookRotation</c>.
        /// </summary>
        public static quaternion LookRotation(float3 forward, float3 up)
        {
            forward = math.normalize(forward);
            var right = math.normalize(math.cross(up, forward));
            up = math.cross(forward, right);

            float m00 = right.x; float m01 = right.y; float m02 = right.z;
            float m10 = up.x;    float m11 = up.y;    float m12 = up.z;
            float m20 = forward.x; float m21 = forward.y; float m22 = forward.z;

            float num8 = m00 + m11 + m22;
            quaternion q = new quaternion();
            if (num8 > 0f)
            {
                float num = math.sqrt(num8 + 1f);
                q.w = num * 0.5f;
                num = 0.5f / num;
                q.x = (m12 - m21) * num;
                q.y = (m20 - m02) * num;
                q.z = (m01 - m10) * num;
                return q;
            }
            if (m00 >= m11 && m00 >= m22)
            {
                float num7 = math.sqrt(((1f + m00) - m11) - m22);
                float num4 = 0.5f / num7;
                q.x = 0.5f * num7;
                q.y = (m01 + m10) * num4;
                q.z = (m02 + m20) * num4;
                q.w = (m12 - m21) * num4;
                return q;
            }
            if (m11 > m22)
            {
                float num6 = math.sqrt(((1f + m11) - m00) - m22);
                float num3 = 0.5f / num6;
                q.x = (m10 + m01) * num3;
                q.y = 0.5f * num6;
                q.z = (m21 + m12) * num3;
                q.w = (m20 - m02) * num3;
                return q;
            }
            float num5 = math.sqrt(((1f + m22) - m00) - m11);
            float num2 = 0.5f / num5;
            q.x = (m20 + m02) * num2;
            q.y = (m21 + m12) * num2;
            q.z = 0.5f * num5;
            q.w = (m01 - m10) * num2;
            return q;
        }
    }
}
