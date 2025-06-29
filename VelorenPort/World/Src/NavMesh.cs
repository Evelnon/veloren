using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.World;

/// <summary>
/// Navigation mesh storing neighbors for each passable cell.
/// Generated from a <see cref="NavGrid"/>.
/// </summary>
[Serializable]
public class NavMesh
{
    private readonly Dictionary<int2, int2[]> _neighbors;

    private class Int2Comparer : IEqualityComparer<int2>
    {
        public bool Equals(int2 a, int2 b) => a.x == b.x && a.y == b.y;
        public int GetHashCode(int2 v) => HashCode.Combine(v.x, v.y);
    }

    private NavMesh(Dictionary<int2, int2[]> neighbors)
    {
        _neighbors = neighbors;
    }

    public IEnumerable<int2> GetNeighbors(int2 pos)
        => _neighbors.TryGetValue(pos, out var list) ? list : Array.Empty<int2>();

    public static NavMesh Generate(NavGrid navGrid)
    {
        var dirs = new int2[]
        {
            new int2(1,0), new int2(-1,0), new int2(0,1), new int2(0,-1),
            new int2(1,1), new int2(1,-1), new int2(-1,1), new int2(-1,-1)
        };
        var neighbors = new Dictionary<int2, int2[]>(new Int2Comparer());
        var size = navGrid.Size;
        for (int y = 0; y < size.y; y++)
        for (int x = 0; x < size.x; x++)
        {
            var pos = new int2(x, y);
            if (navGrid.IsBlocked(pos))
                continue;
            var list = new List<int2>();
            foreach (var d in dirs)
            {
                var nb = pos + d;
                if (!navGrid.IsBlocked(nb))
                    list.Add(nb);
            }
            neighbors[pos] = list.ToArray();
        }
        return new NavMesh(neighbors);
    }
}
