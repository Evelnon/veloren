using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Util
{
    /// <summary>
    /// Simple grid indexed by world coordinates.
    /// </summary>
    public class WGrid<T>
    {
        private readonly uint _cellSize;
        private readonly Grid<T> _grid;

        public WGrid(uint radius, uint cellSize, T defaultCell)
        {
            _cellSize = cellSize;
            int size = (int)(radius * 2 + 1);
            _grid = new Grid<T>(new int2(size, size), defaultCell);
        }

        private int2 Offset => _grid.Size / 2;

        public T? GetLocal(int2 pos) => _grid.Get(pos + Offset);

        public bool SetLocal(int2 pos, T value) => _grid.Set(pos + Offset, value);

        public T? GetWorld(int2 wpos) => GetLocal(wpos / (int)_cellSize);

        public bool SetWorld(int2 wpos, T value) => SetLocal(wpos / (int)_cellSize, value);

        public Grid<T> Grid => _grid;
    }
}
