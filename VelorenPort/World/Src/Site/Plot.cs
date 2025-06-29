using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Site
{
    /// <summary>
    /// Minimal representation of a building or structure within a site.
    /// Serves as a placeholder until the full plot system is ported.
    /// </summary>
    [Serializable]
    public class Plot
    {
        public int2 LocalPos { get; set; }
        public PlotKind Kind { get; set; } = PlotKind.House;
    }

    [Serializable]
    public enum PlotKind
    {
        House,
        AirshipDock,
        GliderRing,
        GliderPlatform,
        GliderFinish,
        Tavern,
        CoastalAirshipDock,
        CoastalHouse,
        CoastalWorkshop,
        Workshop,
        DesertCityMultiPlot,
        DesertCityTemple,
        DesertCityArena,
        DesertCityAirshipDock,
        SeaChapel,
        JungleRuin,
        Plaza,
        Castle,
        Cultist,
        Road,
        Gnarling,
        Adlet,
        Haniwa,
        GiantTree,
        GuardTower,
        CliffTower,
        CliffTownAirshipDock,
        Sahagin,
        Citadel,
        SavannahAirshipDock,
        SavannahHut,
        SavannahWorkshop,
        Barn,
        Bridge,
        PirateHideout,
        RockCircle,
        TrollCave,
        Camp,
        DwarvenMine,
        TerracottaPalace,
        TerracottaHouse,
        TerracottaYard,
        FarmField,
        VampireCastle,
        MyrmidonArena,
        MyrmidonHouse,
        Unknown
    }
}
