using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Site
{
    /// <summary>
    /// Collection of very small plot templates translated from the Rust project.
    /// Only a few patterns are implemented as placeholders for the full set.
    /// </summary>
    public static class PlotTemplates
    {
        /// <summary>
        /// Tile layouts for a subset of plots. These are simplified versions of
        /// the structures defined in the Rust source under <c>world/src/site/plot</c>.
        /// Only a handful of shapes are provided as the full set is extensive.
        /// </summary>
        public static readonly Dictionary<PlotKind, Dictionary<int2, Tile>> Templates = new()
        {
            // A single plaza tile at the origin.
            [PlotKind.Plaza] = new Dictionary<int2, Tile>
            {
                [int2.zero] = Tile.Free(TileKind.Plaza)
            },

            // One road tile. Longer roads are created by applying this template
            // repeatedly in a line.
            [PlotKind.Road] = new Dictionary<int2, Tile>
            {
                [int2.zero] = Tile.Free(TileKind.Road)
            },

            // Basic 2x2 house with the door on the south side. Surrounding tiles
            // are filled in by the generator when placed.
            [PlotKind.House] = new Dictionary<int2, Tile>
            {
                [new int2(0, 0)] = Tile.Free(TileKind.Building),
                [new int2(1, 0)] = Tile.Free(TileKind.Building),
                [new int2(0, 1)] = Tile.Free(TileKind.Building),
                [new int2(1, 1)] = Tile.Free(TileKind.Building),
                [new int2(0, -1)] = Tile.Free(TileKind.Road)
            },

            // Small workshop occupying a 3x2 rectangle.
            [PlotKind.Workshop] = new Dictionary<int2, Tile>
            {
                [new int2(0, 0)] = Tile.Free(TileKind.Building),
                [new int2(1, 0)] = Tile.Free(TileKind.Building),
                [new int2(2, 0)] = Tile.Free(TileKind.Building),
                [new int2(0, 1)] = Tile.Free(TileKind.Building),
                [new int2(1, 1)] = Tile.Free(TileKind.Building),
                [new int2(2, 1)] = Tile.Free(TileKind.Building),
                [new int2(1, -1)] = Tile.Free(TileKind.Road)
            },

            // A simple field for farming plots.
            [PlotKind.FarmField] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Field),
                [new int2(1,0)] = Tile.Free(TileKind.Field),
                [new int2(2,0)] = Tile.Free(TileKind.Field),
                [new int2(0,1)] = Tile.Free(TileKind.Field),
                [new int2(1,1)] = Tile.Free(TileKind.Field),
                [new int2(2,1)] = Tile.Free(TileKind.Field),
                [new int2(0,2)] = Tile.Free(TileKind.Field),
                [new int2(1,2)] = Tile.Free(TileKind.Field),
                [new int2(2,2)] = Tile.Free(TileKind.Field)
            }
        };
    }
}
