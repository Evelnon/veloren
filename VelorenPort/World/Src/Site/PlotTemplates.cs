using System.Collections.Generic;
using VelorenPort.NativeMath;

using VelorenPort.World.Site.Tile;
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
            },

            // Basic watch tower with a 2x2 footprint and a door on the south side.
            [PlotKind.GuardTower] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Tower),
                [new int2(1,0)] = Tile.Free(TileKind.Tower),
                [new int2(0,1)] = Tile.Free(TileKind.Tower),
                [new int2(1,1)] = Tile.Free(TileKind.Tower),
                [new int2(0,-1)] = Tile.Free(TileKind.Road)
            },

            // Simple 3x3 tavern structure.
            [PlotKind.Tavern] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Building),
                [new int2(1,0)] = Tile.Free(TileKind.Building),
                [new int2(2,0)] = Tile.Free(TileKind.Building),
                [new int2(0,1)] = Tile.Free(TileKind.Building),
                [new int2(1,1)] = Tile.Free(TileKind.Building),
                [new int2(2,1)] = Tile.Free(TileKind.Building),
                [new int2(0,2)] = Tile.Free(TileKind.Building),
                [new int2(1,2)] = Tile.Free(TileKind.Building),
                [new int2(2,2)] = Tile.Free(TileKind.Building),
                [new int2(1,-1)] = Tile.Free(TileKind.Road)
            },

            // Small square castle keep.
            [PlotKind.Castle] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Castle),
                [new int2(1,0)] = Tile.Free(TileKind.Castle),
                [new int2(2,0)] = Tile.Free(TileKind.Castle),
                [new int2(3,0)] = Tile.Free(TileKind.Castle),
                [new int2(0,1)] = Tile.Free(TileKind.Castle),
                [new int2(3,1)] = Tile.Free(TileKind.Castle),
                [new int2(0,2)] = Tile.Free(TileKind.Castle),
                [new int2(3,2)] = Tile.Free(TileKind.Castle),
                [new int2(0,3)] = Tile.Free(TileKind.Castle),
                [new int2(1,3)] = Tile.Free(TileKind.Castle),
                [new int2(2,3)] = Tile.Free(TileKind.Castle),
                [new int2(3,3)] = Tile.Free(TileKind.Castle),
                [new int2(1,-1)] = Tile.Free(TileKind.Road)
            },

            // Airship docks share a simple tower template
            [PlotKind.AirshipDock] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Tower),
                [new int2(1,0)] = Tile.Free(TileKind.Tower),
                [new int2(0,1)] = Tile.Free(TileKind.Tower),
                [new int2(1,1)] = Tile.Free(TileKind.Tower),
                [new int2(0,-1)] = Tile.Free(TileKind.Road)
            },

            [PlotKind.Barn] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Building),
                [new int2(1,0)] = Tile.Free(TileKind.Building),
                [new int2(2,0)] = Tile.Free(TileKind.Building),
                [new int2(0,1)] = Tile.Free(TileKind.Building),
                [new int2(1,1)] = Tile.Free(TileKind.Building),
                [new int2(2,1)] = Tile.Free(TileKind.Building),
                [new int2(1,-1)] = Tile.Free(TileKind.Road)
            },

            // Simple one-tile markers for special plots
            [PlotKind.GliderRing] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Field) },
            [PlotKind.GliderPlatform] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Field) },
            [PlotKind.GliderFinish] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Field) },

            [PlotKind.SeaChapel] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Building) },
            [PlotKind.JungleRuin] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Building) },
            [PlotKind.Cultist] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Building) },
            [PlotKind.Gnarling] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.GnarlingFortification) },
            [PlotKind.Adlet] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.AdletStronghold) },
            [PlotKind.Haniwa] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Building) },
            [PlotKind.GiantTree] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Building) },
            [PlotKind.PirateHideout] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Building) },
            [PlotKind.TrollCave] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Building) },
            [PlotKind.Sahagin] = new Dictionary<int2, Tile> { [int2.zero] = Tile.Free(TileKind.Building) },

            [PlotKind.Bridge] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Bridge),
                [new int2(1,0)] = Tile.Free(TileKind.Bridge),
                [new int2(2,0)] = Tile.Free(TileKind.Bridge)
            },

            [PlotKind.Camp] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Field),
                [new int2(1,0)] = Tile.Free(TileKind.Field),
                [new int2(0,1)] = Tile.Free(TileKind.Field),
                [new int2(1,1)] = Tile.Free(TileKind.Field)
            },

            [PlotKind.Citadel] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Castle),
                [new int2(1,0)] = Tile.Free(TileKind.Castle),
                [new int2(0,1)] = Tile.Free(TileKind.Castle),
                [new int2(1,1)] = Tile.Free(TileKind.Castle),
                [new int2(0,-1)] = Tile.Free(TileKind.Road)
            },

            [PlotKind.DwarvenMine] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.DwarvenMine)
            },

            [PlotKind.RockCircle] = new Dictionary<int2, Tile>
            {
                [new int2(0,0)] = Tile.Free(TileKind.Field),
                [new int2(1,0)] = Tile.Free(TileKind.Field),
                [new int2(0,1)] = Tile.Free(TileKind.Field),
                [new int2(1,1)] = Tile.Free(TileKind.Field),
                [new int2(-1,0)] = Tile.Free(TileKind.Field),
                [new int2(0,-1)] = Tile.Free(TileKind.Field),
                [new int2(1,-1)] = Tile.Free(TileKind.Field)
            },

            [PlotKind.TerracottaHouse] = new Dictionary<int2, Tile>(Templates[PlotKind.House]),
            [PlotKind.TerracottaPalace] = new Dictionary<int2, Tile>(Templates[PlotKind.Castle]),
            [PlotKind.TerracottaYard] = new Dictionary<int2, Tile>(Templates[PlotKind.FarmField]),

            [PlotKind.CoastalHouse] = new Dictionary<int2, Tile>(Templates[PlotKind.House]),
            [PlotKind.CoastalWorkshop] = new Dictionary<int2, Tile>(Templates[PlotKind.Workshop]),
            [PlotKind.CoastalAirshipDock] = new Dictionary<int2, Tile>(Templates[PlotKind.AirshipDock]),

            [PlotKind.SavannahAirshipDock] = new Dictionary<int2, Tile>(Templates[PlotKind.AirshipDock]),
            [PlotKind.SavannahHut] = new Dictionary<int2, Tile>(Templates[PlotKind.House]),
            [PlotKind.SavannahWorkshop] = new Dictionary<int2, Tile>(Templates[PlotKind.Workshop]),

            [PlotKind.CliffTower] = new Dictionary<int2, Tile>(Templates[PlotKind.GuardTower]),
            [PlotKind.CliffTownAirshipDock] = new Dictionary<int2, Tile>(Templates[PlotKind.AirshipDock]),
            [PlotKind.DesertCityAirshipDock] = new Dictionary<int2, Tile>(Templates[PlotKind.AirshipDock]),
            [PlotKind.DesertCityMultiPlot] = new Dictionary<int2, Tile>(Templates[PlotKind.Castle]),
            [PlotKind.DesertCityTemple] = new Dictionary<int2, Tile>(Templates[PlotKind.Castle]),
            [PlotKind.DesertCityArena] = new Dictionary<int2, Tile>(Templates[PlotKind.Castle]),

            [PlotKind.VampireCastle] = new Dictionary<int2, Tile>(Templates[PlotKind.Castle]),
            [PlotKind.MyrmidonArena] = new Dictionary<int2, Tile>(Templates[PlotKind.Castle]),
            [PlotKind.MyrmidonHouse] = new Dictionary<int2, Tile>(Templates[PlotKind.House])
        };
    }
}
