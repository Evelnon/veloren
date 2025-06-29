using System;

namespace VelorenPort.World.Site
{
    /// <summary>
    /// Classification of a settlement or landmark. Only a subset of the Rust
    /// enumeration is used so far but keeping the names for compatibility.
    /// </summary>
    [Serializable]
    public enum SiteKind
    {
        Refactor,
        CliffTown,
        SavannahTown,
        DesertCity,
        ChapelSite,
        DwarvenMine,
        CoastalTown,
        Citadel,
        Terracotta,
        GiantTree,
        Gnarling,
        Bridge,
        Adlet,
        Haniwa,
        PirateHideout,
        JungleRuin,
        RockCircle,
        TrollCave,
        Camp,
        Cultist,
        Sahagin,
        VampireCastle,
        GliderCourse,
        Myrmidon,
        Unknown
    }
}
