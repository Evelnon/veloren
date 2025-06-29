using VelorenPort.World.Site.Tile;
using VelorenPort.World.Util;

namespace World.Tests;

public class TileKindTests
{
    [Fact]
    public void EnumIndices_AreStable()
    {
        Assert.Equal(0, EnumIndex.IndexFromEnum(TileKindTag.Empty));
        Assert.Equal(3, EnumIndex.IndexFromEnum(TileKindTag.Plaza));
        Assert.Equal(TileKindTag.Bridge, EnumIndex.EnumFromIndex<TileKindTag>(13));
    }
}
