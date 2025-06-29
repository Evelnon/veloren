using System;

namespace VelorenPort.World.Site
{
    /// <summary>
    /// Metadata describing site categories. Mirrors the Rust
    /// <c>SiteKindMeta</c> hierarchy used by other modules.
    /// </summary>
    [Serializable]
    public abstract record SiteKindMeta
    {
        public sealed record Dungeon(DungeonKindMeta Kind) : SiteKindMeta;
        public sealed record Cave : SiteKindMeta;
        public sealed record Settlement(SettlementKindMeta Kind) : SiteKindMeta;
        public sealed record Castle : SiteKindMeta;
        public sealed record Void : SiteKindMeta;
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
}
