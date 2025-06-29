using System;

namespace VelorenPort.World.Site.Tile {
    /// <summary>
    /// Simplified representation of a site tile. This mirrors part of
    /// `world/src/site/tile.rs` and is used by generators to mark
    /// terrain for roads, buildings and similar structures.
    /// </summary>
    [Serializable]
    public class Tile {
        public TileKind Kind { get; set; } = TileKind.Empty;

        /// <summary>
        /// Optional identifier of a plot this tile belongs to. The full plot
        /// system has not been ported yet so this is simply a numeric ID.
        /// </summary>
        public ulong? PlotId { get; set; } = null;

        /// <summary>
        /// If set, the tile altitude should be forced to this value during
        /// generation.
        /// </summary>
        public int? HardAlt { get; set; } = null;

        public static Tile Empty => new Tile { Kind = TileKind.Empty };

        public static Tile Free(TileKind kind) => new Tile { Kind = kind };

        public bool IsEmpty => Kind == TileKind.Empty;
        public bool IsNatural => Kind == TileKind.Empty || Kind == TileKind.Hazard;
        public bool IsRoad => Kind is TileKind.Plaza or TileKind.Road or TileKind.Path;
        public bool IsBuilding => Kind is TileKind.Building or TileKind.Castle or TileKind.Wall;
    }

    /// <summary>Tile category used when marking site layouts.</summary>
    [Serializable]
    public enum TileKind {
        Empty,
        Hazard,
        Field,
        Plaza,
        Road,
        Path,
        Building,
        Castle,
        Wall,
        Tower,
        Keep,
        Gate,
        GnarlingFortification,
        Bridge,
        AdletStronghold,
        DwarvenMine,
    }

    /// <summary>Type of hazardous tile.</summary>
    [Serializable]
    public enum HazardKind {
        Water,
        Hill,
    }

    /// <summary>Type of castle keep used by <see cref="TileKind.Keep"/>.</summary>
    [Serializable]
    public enum KeepKind {
        Middle,
        Corner,
        Wall,
    }

    /// <summary>Roof decoration for towers.</summary>
    [Serializable]
    public enum RoofKind {
        Parapet,
        Pyramid,
    }
}
