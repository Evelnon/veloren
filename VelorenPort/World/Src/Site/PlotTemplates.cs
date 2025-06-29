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
        public static readonly Dictionary<PlotKind, Dictionary<int2, Tile>> Templates = new()
        {
            [PlotKind.Plaza] = new Dictionary<int2, Tile>
            {
                [int2.zero] = Tile.Free(TileKind.Plaza)
            },
            [PlotKind.Road] = new Dictionary<int2, Tile>
            {
                [int2.zero] = Tile.Free(TileKind.Road)
            }
        };
    }
}
