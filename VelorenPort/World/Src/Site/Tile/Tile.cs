using System;
using VelorenPort.NativeMath;
using VelorenPort.World.Site.Util;
using VelorenPort.World.Sim;

namespace VelorenPort.World.Site.Tile;

/// <summary>
/// Representation of a site tile mirroring <c>world/src/site/tile.rs</c>.
/// </summary>
[Serializable]
public class Tile
{
    public TileKind Kind { get; set; } = TileKind.Empty;

    /// <summary>Optional identifier of the plot this tile belongs to.</summary>
    public ulong? PlotId { get; set; } = null;

    /// <summary>Forced altitude if specified.</summary>
    public int? HardAlt { get; set; } = null;

    public static Tile Empty => new Tile { Kind = TileKind.Empty };
    public static Tile Free(TileKind kind) => new Tile { Kind = kind };

    public bool IsEmpty => Kind.Tag == TileKindTag.Empty;
    public bool IsNatural => Kind.Tag == TileKindTag.Empty || Kind.Tag == TileKindTag.Hazard;
    public bool IsRoad => Kind.Tag is TileKindTag.Plaza or TileKindTag.Road or TileKindTag.Path;
    public bool IsBuilding => Kind.Tag is TileKindTag.Building or TileKindTag.Castle or TileKindTag.Wall;
}

/// <summary>Discriminant for <see cref="TileKind"/>.</summary>
[Serializable]
public enum TileKindTag
{
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

[Serializable]
public struct RoadData
{
    public ushort A;
    public ushort B;
    public ushort W;
}

[Serializable]
public struct PathData
{
    public float2 ClosestPos;
    public Path Path;
}

/// <summary>
/// Tile category with variant data.
/// </summary>
[Serializable]
public struct TileKind
{
    public TileKindTag Tag;
    public HazardKind? Hazard;
    public RoadData? Road;
    public PathData? Path;
    public Dir? WallDir;
    public RoofKind? Roof;
    public KeepKind? Keep;

    private TileKind(TileKindTag tag)
    {
        Tag = tag;
        Hazard = null;
        Road = null;
        Path = null;
        WallDir = null;
        Roof = null;
        Keep = null;
    }

    public static TileKind Empty => new TileKind(TileKindTag.Empty);
    public static TileKind Field => new TileKind(TileKindTag.Field);
    public static TileKind Plaza => new TileKind(TileKindTag.Plaza);
    public static TileKind Building => new TileKind(TileKindTag.Building);
    public static TileKind Castle => new TileKind(TileKindTag.Castle);
    public static TileKind Gate => new TileKind(TileKindTag.Gate);
    public static TileKind GnarlingFortification => new TileKind(TileKindTag.GnarlingFortification);
    public static TileKind Bridge => new TileKind(TileKindTag.Bridge);
    public static TileKind AdletStronghold => new TileKind(TileKindTag.AdletStronghold);
    public static TileKind DwarvenMine => new TileKind(TileKindTag.DwarvenMine);

    public static TileKind HazardTile(HazardKind kind) => new TileKind(TileKindTag.Hazard) { Hazard = kind };
    public static TileKind RoadTile(ushort a, ushort b, ushort w) => new TileKind(TileKindTag.Road) { Road = new RoadData { A = a, B = b, W = w } };
    public static TileKind PathTile(float2 pos, Path path) => new TileKind(TileKindTag.Path) { Path = new PathData { ClosestPos = pos, Path = path } };
    public static TileKind WallTile(Dir dir) => new TileKind(TileKindTag.Wall) { WallDir = dir };
    public static TileKind TowerTile(RoofKind roof) => new TileKind(TileKindTag.Tower) { Roof = roof };
    public static TileKind KeepTile(KeepKind keep) => new TileKind(TileKindTag.Keep) { Keep = keep };
}

/// <summary>Type of hazardous tile.</summary>
[Serializable]
public enum HazardKind { Water, Hill }

/// <summary>Type of castle keep used by <see cref="TileKindTag.Keep"/>.</summary>
[Serializable]
public enum KeepKind { Middle, Corner, Wall }

/// <summary>Roof decoration for towers.</summary>
[Serializable]
public enum RoofKind { Parapet, Pyramid }

