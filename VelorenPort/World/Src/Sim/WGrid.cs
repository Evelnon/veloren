using System;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Sim;

/// <summary>
/// Small wrapper around <see cref="Grid{T}"/> with a fixed cell size and an
/// origin in the center. Provides local coordinate access similar to
/// world/src/util/wgrid.rs.
/// </summary>
public class WGrid<T>
{
    private readonly int cellSize;
    private readonly Grid<T> grid;

    public WGrid(int radius, int cellSize, T defaultCell)
    {
        this.cellSize = cellSize;
        grid = new Grid<T>(new int2(radius * 2 + 1, radius * 2 + 1), defaultCell);
    }

    private int2 Offset => grid.Size / 2;

    public T? GetLocal(int2 pos) => grid.Get(pos + Offset);

    public void SetLocal(int2 pos, T value) => grid.Set(pos + Offset, value);
}
