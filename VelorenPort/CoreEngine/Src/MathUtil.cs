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
    }
}
