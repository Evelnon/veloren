using VelorenPort.World.Site.Tile;
using VelorenPort.World.Util;

namespace World.Tests;

public class TileKindTests
{
    [Fact]
    public void EnumIndices_AreStable()
    {
        Assert.Equal(0, EnumIndex.IndexFromEnum(TileKind.Empty));
        Assert.Equal(3, EnumIndex.IndexFromEnum(TileKind.Plaza));
        Assert.Equal(TileKind.Bridge, EnumIndex.EnumFromIndex<TileKind>(13));
    }
}
