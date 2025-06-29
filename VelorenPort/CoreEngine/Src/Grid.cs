using System;
using System.Collections.Generic;
using System.Linq;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Generic 2D grid similar to the utility in common/src/grid.rs.
    /// Provides indexed access and helpers to iterate areas.
    /// </summary>
    [Serializable]
    public class Grid<T> {
        private readonly T[] _cells;
        private readonly int2 _size;

        public Grid(int2 size, T defaultCell) {
            _size = size;
            _cells = Enumerable.Repeat(defaultCell, size.x * size.y).ToArray();
        }

        public Grid(int2 size, IEnumerable<T> raw) {
            var data = raw.ToArray();
            if (data.Length != size.x * size.y)
                throw new ArgumentException("Raw data size mismatch");
            _size = size;
            _cells = data;
        }

        public static Grid<T> PopulateFrom(int2 size, Func<int2, T> f) {
            var data = new T[size.x * size.y];
            int i = 0;
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                data[i++] = f(new int2(x, y));
            return new Grid<T>(size, data);
        }

        private int? Idx(int2 pos) {
            if (pos.x >= 0 && pos.y >= 0 && pos.x < _size.x && pos.y < _size.y)
                return pos.y * _size.x + pos.x;
            return null;
        }

        public int2 Size => _size;

        public T? Get(int2 pos) {
            int? idx = Idx(pos);
            return idx.HasValue ? _cells[idx.Value] : default;
        }

        public bool TryGet(int2 pos, out T value) {
            int? idx = Idx(pos);
            if (idx.HasValue) { value = _cells[idx.Value]; return true; }
            value = default!; return false;
        }

        public bool Set(int2 pos, T value) {
            int? idx = Idx(pos);
            if (idx.HasValue) { _cells[idx.Value] = value; return true; }
            return false;
        }

        public IEnumerable<(int2 Pos, T Cell)> Iterate() {
            for (int y = 0; y < _size.y; y++)
            for (int x = 0; x < _size.x; x++) {
                var pos = new int2(x, y);
                yield return (pos, _cells[y * _size.x + x]);
            }
        }

        public IEnumerable<(int2 Pos, T Cell)> IterateArea(int2 pos, int2 size) {
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++) {
                var p = new int2(pos.x + x, pos.y + y);
                int? idx = Idx(p);
                if (idx.HasValue)
                    yield return (p, _cells[idx.Value]);
            }
        }

        public T this[int2 pos] {
            get {
                int? idx = Idx(pos);
                if (!idx.HasValue) throw new IndexOutOfRangeException();
                return _cells[idx.Value];
            }
            set {
                int? idx = Idx(pos);
                if (!idx.HasValue) throw new IndexOutOfRangeException();
                _cells[idx.Value] = value;
            }
        }
    }
}
