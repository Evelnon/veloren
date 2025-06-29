using VelorenPort.World.Site;
using VelorenPort.World;
using Unity.Mathematics;

namespace World.Tests;

public class SiteKindTests
{
    [Fact]
    public void MarkerMapping_ReturnsExpected()
    {
        Assert.Equal(MarkerKind.Town, SiteKindExtensions.Marker(SiteKind.CliffTown));
        Assert.Equal(MarkerKind.Castle, SiteKindExtensions.Marker(SiteKind.Citadel));
        Assert.Null(SiteKindExtensions.Marker(SiteKind.PirateHideout));
    }

    [Fact]
    public void MetaConversion_ReturnsExpected()
    {
        var meta = SiteKindExtensions.Meta(SiteKind.CliffTown) as SiteKindMeta.Settlement;
        Assert.NotNull(meta);
        Assert.Equal(SettlementKindMeta.CliffTown, meta!.Kind);
    }

    [Fact]
    public void WorldMap_IncludesKinds()
    {
        var (world, index) = World.Empty();
        var site = new Site { Position = int2.zero, Name = "A", Kind = SiteKind.Citadel };
        index.Sites.Insert(site);
        var map = world.GetMapData();
        Assert.Single(map.Sites);
        Assert.Equal(MarkerKind.Castle, map.Sites[0].Kind);
    }
}

