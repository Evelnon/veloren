using System;

namespace VelorenPort.World.Site
{
    /// <summary>
    /// Helper conversions for <see cref="SiteKind"/>.
    /// </summary>
    public static class SiteKindExtensions
    {
        public static SiteKindMeta? Meta(this SiteKind kind) => kind switch
        {
            SiteKind.Refactor => new SiteKindMeta.Settlement(SettlementKindMeta.Default),
            SiteKind.CliffTown => new SiteKindMeta.Settlement(SettlementKindMeta.CliffTown),
            SiteKind.SavannahTown => new SiteKindMeta.Settlement(SettlementKindMeta.SavannahTown),
            SiteKind.CoastalTown => new SiteKindMeta.Settlement(SettlementKindMeta.CoastalTown),
            SiteKind.DesertCity => new SiteKindMeta.Settlement(SettlementKindMeta.DesertCity),
            SiteKind.Citadel => new SiteKindMeta.Castle(),
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
            _ => null
        };

        public static bool ShouldDoEconomicSimulation(this SiteKind kind) => kind switch
        {
            SiteKind.Refactor or SiteKind.CliffTown or SiteKind.SavannahTown or SiteKind.CoastalTown or SiteKind.DesertCity => true,
            _ => false,
        };

        public static MarkerKind ToMarker(this SiteKind kind) => kind switch
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
            _ => MarkerKind.Unknown,
        };
    }
}
