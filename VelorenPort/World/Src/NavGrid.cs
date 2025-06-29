using System;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.World;

/// <summary>
/// Simple navigation grid storing which chunk cells are blocked.
/// Can be used by <see cref="Searcher"/> to avoid impassable areas.
/// </summary>
[Serializable]
public class NavGrid
{
    private readonly Grid<bool> _grid;

    public NavGrid(int2 size)
    {
        _grid = new Grid<bool>(size, false);
    }

    public int2 Size => _grid.Size;

    /// <summary>Return true if the cell is blocked. Out of bounds count as blocked.</summary>
    public bool IsBlocked(int2 pos)
    {
        var v = _grid.Get(pos);
        return v.HasValue ? v.Value : true;
    }

    public void SetBlocked(int2 pos, bool blocked) => _grid.Set(pos, blocked);
}
