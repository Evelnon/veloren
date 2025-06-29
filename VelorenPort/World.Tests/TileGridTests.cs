using VelorenPort.World.Site;
using VelorenPort.NativeMath;

namespace World.Tests;

public class TileGridTests
{
    [Fact]
    public void Set_AddsTileAndExpandsBounds()
    {
        var grid = new TileGrid();
        grid.Set(new int2(1, 2), Tile.Free(TileKind.Plaza));
        Assert.Equal(TileKind.Plaza, grid.Get(new int2(1, 2)).Kind);
        Assert.Equal(new int2(1, 2), grid.Bounds.Min);
        Assert.Equal(new int2(2, 3), grid.Bounds.Max);
    }

    [Fact]
    public void GrowAabr_ExpandsToRequestedArea()
    {
        var grid = new TileGrid();
        var (ok, aabr) = grid.GrowAabr(int2.zero, 3, 9, new int2(1,1));
        Assert.True(ok);
        Assert.Equal(new int2(-1, -1), aabr.Min);
        Assert.Equal(new int2(2, 2), aabr.Max);
    }

    [Fact]
    public void GrowOrganic_ProducesTileSet()
    {
        var grid = new TileGrid();
        var rng = new System.Random(1);
        var (ok, set) = grid.GrowOrganic(rng, int2.zero, 3, 5);
        Assert.True(ok);
        Assert.True(set.Count >= 3 && set.Count <= 5);
        Assert.Contains(int2.zero, set);
    }
}
