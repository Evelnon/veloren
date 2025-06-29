using System;
using Unity.Mathematics;

namespace VelorenPort.World.Site;

/// <summary>
/// Enumerations describing types of sites and related metadata.
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
}

[Serializable]
public enum MarkerKind
{
    Town,
    Castle,
    Cave,
    Tree,
    Gnarling,
    GliderCourse,
    ChapelSite,
    Terracotta,
    Bridge,
    Adlet,
    Haniwa,
    DwarvenMine,
    Cultist,
    Sahagin,
    VampireCastle,
    Myrmidon,
    Unknown,
}

[Serializable]
public enum DungeonKindMeta
{
    Gnarling,
    Adlet,
    Haniwa,
    SeaChapel,
    Terracotta,
    Cultist,
    Sahagin,
    Myrmidon,
    VampireCastle,
    DwarvenMine,
}

[Serializable]
public enum SettlementKindMeta
{
    Default,
    CliffTown,
    DesertCity,
    SavannahTown,
    CoastalTown,
}

[Serializable]
public abstract record SiteKindMeta
{
    public sealed record Dungeon(DungeonKindMeta Kind) : SiteKindMeta;
    public sealed record Cave : SiteKindMeta;
    public sealed record Settlement(SettlementKindMeta Kind) : SiteKindMeta;
    public sealed record Castle : SiteKindMeta;
    public sealed record Void : SiteKindMeta;
}

public static class SiteKindExtensions
{
    public static SiteKindMeta? Meta(this SiteKind kind) => kind switch
    {
        SiteKind.Refactor => new SiteKindMeta.Settlement(SettlementKindMeta.Default),
        SiteKind.CliffTown => new SiteKindMeta.Settlement(SettlementKindMeta.CliffTown),
        SiteKind.SavannahTown => new SiteKindMeta.Settlement(SettlementKindMeta.SavannahTown),
        SiteKind.CoastalTown => new SiteKindMeta.Settlement(SettlementKindMeta.CoastalTown),
        SiteKind.DesertCity => new SiteKindMeta.Settlement(SettlementKindMeta.DesertCity),
        SiteKind.Gnarling => new SiteKindMeta.Dungeon(DungeonKindMeta.Gnarling),
        SiteKind.Adlet => new SiteKindMeta.Dungeon(DungeonKindMeta.Adlet),
        SiteKind.Terracotta => new SiteKindMeta.Dungeon(DungeonKindMeta.Terracotta),
        SiteKind.Haniwa => new SiteKindMeta.Dungeon(DungeonKindMeta.Haniwa),
        SiteKind.Myrmidon => new SiteKindMeta.Dungeon(DungeonKindMeta.Myrmidon),
        SiteKind.DwarvenMine => new SiteKindMeta.Dungeon(DungeonKindMeta.DwarvenMine),
        SiteKind.ChapelSite => new SiteKindMeta.Dungeon(DungeonKindMeta.SeaChapel),
        SiteKind.Cultist => new SiteKindMeta.Dungeon(DungeonKindMeta.Cultist),
        SiteKind.Sahagin => new SiteKindMeta.Dungeon(DungeonKindMeta.Sahagin),
        SiteKind.VampireCastle => new SiteKindMeta.Dungeon(DungeonKindMeta.VampireCastle),
        _ => null,
    };

    public static bool ShouldDoEconomicSimulation(this SiteKind kind) => kind switch
    {
        SiteKind.Refactor or SiteKind.CliffTown or SiteKind.SavannahTown or SiteKind.CoastalTown or SiteKind.DesertCity => true,
        _ => false,
    };

    public static MarkerKind? Marker(this SiteKind kind) => kind switch
    {
        SiteKind.Refactor or SiteKind.CliffTown or SiteKind.SavannahTown or SiteKind.CoastalTown or SiteKind.DesertCity => MarkerKind.Town,
        SiteKind.Citadel => MarkerKind.Castle,
        SiteKind.Bridge => MarkerKind.Bridge,
        SiteKind.GiantTree => MarkerKind.Tree,
        SiteKind.Gnarling => MarkerKind.Gnarling,
        SiteKind.DwarvenMine => MarkerKind.DwarvenMine,
        SiteKind.ChapelSite => MarkerKind.ChapelSite,
        SiteKind.Terracotta => MarkerKind.Terracotta,
        SiteKind.GliderCourse => MarkerKind.GliderCourse,
        SiteKind.Cultist => MarkerKind.Cultist,
        SiteKind.Sahagin => MarkerKind.Sahagin,
        SiteKind.Myrmidon => MarkerKind.Myrmidon,
        SiteKind.Adlet => MarkerKind.Adlet,
        SiteKind.Haniwa => MarkerKind.Haniwa,
        SiteKind.VampireCastle => MarkerKind.VampireCastle,
        _ => null,
    };
}

/// <summary>
/// Point of interest kinds mirroring Rust's `PoiKind` union.
/// </summary>
[Serializable]
public abstract record PoiKind
{
    public sealed record Peak(uint Altitude) : PoiKind;
    public sealed record Lake(uint Size) : PoiKind;
}

