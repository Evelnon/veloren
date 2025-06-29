using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using Unity.Entities;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Simple spatial grid for broad phase queries. Mirrors util/spatial_grid.rs
    /// but adapted to idiomatic C# using dictionaries keyed by int2.
    /// </summary>
    [Serializable]
    public class SpatialGrid {
        private readonly Dictionary<int2, List<Entity>> _grid;
        private readonly Dictionary<int2, List<Entity>> _largeGrid;
        private readonly int _lg2CellSize;
        private readonly int _lg2LargeCellSize;
        private readonly uint _radiusCutoff;
        private uint _largestLargeRadius;

        private struct Int2Comparer : IEqualityComparer<int2> {
            public bool Equals(int2 a, int2 b) => a.x == b.x && a.y == b.y;
            public int GetHashCode(int2 v) => HashCode.Combine(v.x, v.y);
        }

        public SpatialGrid(int lg2CellSize, int lg2LargeCellSize, uint radiusCutoff) {
            _grid = new Dictionary<int2, List<Entity>>(new Int2Comparer());
            _largeGrid = new Dictionary<int2, List<Entity>>(new Int2Comparer());
            _lg2CellSize = lg2CellSize;
            _lg2LargeCellSize = lg2LargeCellSize;
            _radiusCutoff = radiusCutoff;
            _largestLargeRadius = radiusCutoff;
        }

        public void Insert(int2 pos, uint radius, Entity entity) {
            if (radius <= _radiusCutoff) {
                var cell = new int2(pos.x >> _lg2CellSize, pos.y >> _lg2CellSize);
                if (!_grid.TryGetValue(cell, out var list)) {
                    list = new List<Entity>();
                    _grid[cell] = list;
                }
                list.Add(entity);
            } else {
                var cell = new int2(pos.x >> _lg2LargeCellSize, pos.y >> _lg2LargeCellSize);
                if (!_largeGrid.TryGetValue(cell, out var list)) {
                    list = new List<Entity>();
                    _largeGrid[cell] = list;
                }
                list.Add(entity);
                _largestLargeRadius = math.max(_largestLargeRadius, radius);
            }
        }

        private IEnumerable<Entity> Iterate(Aabr aabr, uint maxEntityRadius, Dictionary<int2, List<Entity>> grid, int lg2CellSize) {
            int2 min = aabr.Min - (int)maxEntityRadius;
            int2 max = aabr.Max + (int)maxEntityRadius;
            int2 cellMin = new int2(min.x >> lg2CellSize, min.y >> lg2CellSize);
            int2 cellMax = new int2((max.x + (1 << lg2CellSize) - 1) >> lg2CellSize,
                                    (max.y + (1 << lg2CellSize) - 1) >> lg2CellSize);

            for (int x = cellMin.x; x <= cellMax.x; x++)
            for (int y = cellMin.y; y <= cellMax.y; y++) {
                var cell = new int2(x, y);
                if (grid.TryGetValue(cell, out var list))
                    foreach (var e in list)
                        yield return e;
            }
        }

        public IEnumerable<Entity> InAabr(Aabr aabr) {
            foreach (var e in Iterate(aabr, _radiusCutoff, _grid, _lg2CellSize))
                yield return e;
            foreach (var e in Iterate(aabr, _largestLargeRadius, _largeGrid, _lg2LargeCellSize))
                yield return e;
        }

        public IEnumerable<Entity> InCircleAabr(float2 center, float radius) {
            int2 c = new int2((int)center.x, (int)center.y);
            int r = (int)math.ceil(radius);
            const int CENTER_TRUNCATION_ERROR = 1;
            int maxDist = r + CENTER_TRUNCATION_ERROR;
            var aabr = new Aabr(c - maxDist, c + maxDist);
            return InAabr(aabr);
        }

        public void Clear() {
            _grid.Clear();
            _largeGrid.Clear();
            _largestLargeRadius = _radiusCutoff;
        }
    }

    /// <summary>
    /// Axis-aligned bounding rectangle of integer coordinates.
    /// </summary>
    [Serializable]
    public struct Aabr {
        public int2 Min;
        public int2 Max;
        public Aabr(int2 min, int2 max) { Min = min; Max = max; }
    }
}
