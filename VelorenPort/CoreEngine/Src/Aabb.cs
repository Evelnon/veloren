using System;
using Unity.Mathematics;

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
    }
}
