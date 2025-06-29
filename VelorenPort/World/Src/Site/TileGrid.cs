using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site {
    /// <summary>
    /// Sparse grid storing tiles around a site. Implements a small subset of the
    /// Rust <c>TileGrid</c> used for town generation.
    /// </summary>
    [Serializable]
    public class TileGrid {
        public const int TileSize = 6;
        public const int ZoneSize = 16;
        public const int ZoneRadius = 16;
        public const int TileRadius = ZoneSize * ZoneRadius;
        public const int MaxBlockRadius = TileSize * TileRadius;

        private readonly Dictionary<int2, Tile> _tiles = new();
        public Aabr Bounds { get; private set; } = new Aabr(new int2(int.MaxValue, int.MaxValue), new int2(int.MinValue, int.MinValue));

        private void ExpandBounds(int2 pos) {
            if (Bounds.Min.x > Bounds.Max.x) {
                Bounds = new Aabr(pos, pos + 1);
            } else {
                Bounds = new Aabr(math.min(Bounds.Min, pos), math.max(Bounds.Max, pos + 1));
            }
        }

        /// <summary>Retrieve a tile if one exists at <paramref name="tpos"/>.</summary>
        public Tile? GetKnown(int2 tpos) => _tiles.TryGetValue(tpos, out var t) ? t : null;

        /// <summary>Retrieve the tile at <paramref name="tpos"/> or an empty tile.</summary>
        public Tile Get(int2 tpos) => GetKnown(tpos) ?? Tile.Empty;

        /// <summary>Set the tile at <paramref name="tpos"/> and return the previous value if any.</summary>
        public Tile? Set(int2 tpos, Tile tile) {
            ExpandBounds(tpos);
            if (_tiles.TryGetValue(tpos, out var old)) {
                _tiles[tpos] = tile;
                return old;
            }
            _tiles[tpos] = tile;
            return null;
        }

        /// <summary>
        /// Search outward from <paramref name="center"/> for a tile that satisfies <paramref name="predicate"/>.
        /// Returns the result along with the position if found.
        /// </summary>
        public (T result, int2 pos)? FindNear<T>(int2 center, Func<int2, Tile, T?> predicate) where T : class {
            const int maxRadius = MaxBlockRadius / TileSize; // 96
            foreach (var offs in Spiral.WithRadius(maxRadius)) {
                int2 pos = center + offs;
                if (_tiles.TryGetValue(pos, out var t)) {
                    var r = predicate(pos, t);
                    if (r != null) return (r, pos);
                }
            }
            return null;
        }

        /// <summary>
        /// Expand a rectangle around <paramref name="center"/> while the area
        /// stays below <paramref name="maxArea"/> and all newly covered tiles
        /// are empty. Returns the resulting bounds and whether the minimum
        /// <paramref name="minArea"/> and <paramref name="minDims"/> were met.
        /// </summary>
        public (bool success, Aabr bounds) GrowAabr(int2 center, int minArea, int maxArea, int2 minDims)
        {
            var aabr = new Aabr(center, center + 1);
            if (!Get(center).IsEmpty)
                return (false, aabr);

            int lastGrowth = 0;
            for (int i = 0; i < 32; i++)
            {
                int width = aabr.Max.x - aabr.Min.x;
                int height = aabr.Max.y - aabr.Min.y;
                int area = width * height;
                if (i - lastGrowth >= 4 || area + (i % 2 == 0 ? height : width) > maxArea)
                    break;

                int dir = (i + math.abs(center.x + center.y)) % 4;
                bool canGrow = true;
                switch (dir)
                {
                    case 0:
                        for (int y = aabr.Min.y; y < aabr.Max.y; y++)
                            if (!Get(new int2(aabr.Max.x, y)).IsEmpty) { canGrow = false; break; }
                        if (canGrow) { aabr.Max.x += 1; lastGrowth = i; }
                        break;
                    case 1:
                        for (int x = aabr.Min.x; x < aabr.Max.x; x++)
                            if (!Get(new int2(x, aabr.Max.y)).IsEmpty) { canGrow = false; break; }
                        if (canGrow) { aabr.Max.y += 1; lastGrowth = i; }
                        break;
                    case 2:
                        for (int y = aabr.Min.y; y < aabr.Max.y; y++)
                            if (!Get(new int2(aabr.Min.x - 1, y)).IsEmpty) { canGrow = false; break; }
                        if (canGrow) { aabr.Min.x -= 1; lastGrowth = i; }
                        break;
                    default:
                        for (int x = aabr.Min.x; x < aabr.Max.x; x++)
                            if (!Get(new int2(x, aabr.Min.y - 1)).IsEmpty) { canGrow = false; break; }
                        if (canGrow) { aabr.Min.y -= 1; lastGrowth = i; }
                        break;
                }
            }

            int finalWidth = aabr.Max.x - aabr.Min.x;
            int finalHeight = aabr.Max.y - aabr.Min.y;
            int finalArea = finalWidth * finalHeight;
            bool success = finalArea >= minArea && finalWidth >= minDims.x && finalHeight >= minDims.y;
            return (success, aabr);
        }

        /// <summary>
        /// Grow an organic blob of tiles outward from <paramref name="center"/>
        /// using a random flood fill. Tiles already occupied are ignored.
        /// </summary>
        public (bool success, HashSet<int2> tiles) GrowOrganic(Random rng, int2 center, int minArea, int maxArea)
        {
            var result = new HashSet<int2>();
            var open = new List<int2>();

            result.Add(center);
            open.Add(center);

            while (result.Count < maxArea && open.Count > 0)
            {
                int idx = rng.Next(open.Count);
                var tile = open[idx];
                open.RemoveAt(idx);

                foreach (var dir in WorldUtil.CARDINALS)
                {
                    int2 n = tile + dir;
                    if (!Get(n).IsEmpty || result.Contains(n))
                        continue;
                    result.Add(n);
                    open.Add(n);
                    if (result.Count >= maxArea)
                        break;
                }
            }

            bool success = result.Count >= minArea;
            return (success, result);
        }
    }
}
