using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Simple hash for grid coordinates, mirroring util/grid_hasher.rs.
    /// </summary>
    public struct GridHasher : IEqualityComparer<int3>
    {
        public bool Equals(int3 a, int3 b) => a.x == b.x && a.y == b.y && a.z == b.z;

        public int GetHashCode(int3 v)
        {
            ulong h = 0;
            h = h * 113989ul ^ (uint)v.x;
            h = h * 113989ul ^ (uint)v.y;
            h = h * 113989ul ^ (uint)v.z;
            return (int)h;
        }
    }
}
