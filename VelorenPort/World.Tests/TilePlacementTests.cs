using VelorenPort.World.Site;
using VelorenPort.World.Site.Tile;
using VelorenPort.NativeMath;

namespace World.Tests;

public class TilePlacementTests
{
    [Fact]
    public void ApplyTemplate_PlacesTilesAtOrigin()
    {
        var grid = new TileGrid();
        var tmpl = PlotTemplates.Templates[PlotKind.Plaza];
        grid.ApplyTemplate(int2.zero, tmpl);
        Assert.Equal(TileKind.Plaza, grid.Get(int2.zero).Kind);
    }
}
