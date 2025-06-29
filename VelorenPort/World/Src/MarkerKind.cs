using System;

namespace VelorenPort.World
{
    /// <summary>
    /// Kind of marker used on world maps for sites. Mirrors the Rust enum
    /// <c>MarkerKind</c> but only a subset is used so far.
    /// </summary>
    [Serializable]
    public enum MarkerKind : byte
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
        Unknown
    }
}
