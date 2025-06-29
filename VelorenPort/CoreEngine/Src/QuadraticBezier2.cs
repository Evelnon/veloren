using System;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Basic quadratic Bézier curve utilities.
    /// </summary>
    [Serializable]
    public struct QuadraticBezier2
    {
        public float2 Start;
        public float2 Ctrl;
        public float2 End;

        public float2 Evaluate(float t)
        {
            float u = 1f - t;
            return Start * (u * u) + Ctrl * (2f * u * t) + End * (t * t);
        }

        public float2 EvaluateDerivative(float t)
        {
            return (Ctrl - Start) * (2f * (1f - t)) + (End - Ctrl) * (2f * t);
        }

        /// <summary>
        /// Approximate the parameter corresponding to the point on the curve
        /// nearest to <paramref name="target"/>.
        /// </summary>
        public (float t, float2 point) BinarySearchPointBySteps(float2 target, int steps, float epsilon)
        {
            float bestT = 0f;
            float bestDist = float.MaxValue;
            int divisions = 1 << steps;
            for (int i = 0; i <= divisions; i++)
            {
                float t = i / (float)divisions;
                float2 p = Evaluate(t);
                float dist = math.lengthsq(p - target);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestT = t;
                }
            }
            return (bestT, Evaluate(bestT));
        }
    }
}
