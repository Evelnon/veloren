using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.World;

/// <summary>
/// Very small navigation mesh using voxel nodes. Each voxel can
/// connect to its six axis-aligned neighbors when those cells are
/// passable.
/// </summary>
[Serializable]
public class NavMesh
{
    private readonly Dictionary<int3, int3[]> _neighbors;

    private class Int3Comparer : IEqualityComparer<int3>
    {
        public bool Equals(int3 a, int3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public int GetHashCode(int3 v) => HashCode.Combine(v.x, v.y, v.z);
    }

    private NavMesh(Dictionary<int3, int3[]> neighbors)
    {
        _neighbors = neighbors;
    }

    /// <summary>Return the neighbors of a voxel position.</summary>
    public IEnumerable<int3> GetNeighbors(int3 pos)
        => _neighbors.TryGetValue(pos, out var list) ? list : Array.Empty<int3>();

    /// <summary>Create a mesh from a 3D boolean array where true marks walkable cells.</summary>
    public static NavMesh Generate(bool[,,] open)
    {
        var sizeX = open.GetLength(0);
        var sizeY = open.GetLength(1);
        var sizeZ = open.GetLength(2);
        var dirs = new int3[]
        {
            new int3(1,0,0), new int3(-1,0,0),
            new int3(0,1,0), new int3(0,-1,0),
            new int3(0,0,1), new int3(0,0,-1)
        };
        var neighbors = new Dictionary<int3, int3[]>(new Int3Comparer());

        for (int x = 0; x < sizeX; x++)
        for (int y = 0; y < sizeY; y++)
        for (int z = 0; z < sizeZ; z++)
        {
            if (!open[x, y, z])
                continue;
            var pos = new int3(x, y, z);
            var list = new List<int3>();
            foreach (var d in dirs)
            {
                var nb = pos + d;
                if (nb.x < 0 || nb.y < 0 || nb.z < 0 ||
                    nb.x >= sizeX || nb.y >= sizeY || nb.z >= sizeZ)
                    continue;
                if (open[nb.x, nb.y, nb.z])
                    list.Add(nb);
            }
            neighbors[pos] = list.ToArray();
        }

        return new NavMesh(neighbors);
    }

    /// <summary>Create a mesh from a 2D <see cref="NavGrid"/>.</summary>
    public static NavMesh Generate(NavGrid navGrid)
    {
        var size = navGrid.Size;
        var open = new bool[size.x, size.y, 1];
        for (int y = 0; y < size.y; y++)
        for (int x = 0; x < size.x; x++)
            open[x, y, 0] = !navGrid.IsBlocked(new int2(x, y));
        return Generate(open);
    }
}
