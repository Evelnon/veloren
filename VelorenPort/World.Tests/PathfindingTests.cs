using VelorenPort.World;
using Unity.Mathematics;
using Xunit;

namespace World.Tests;

public class PathfindingTests
{
    [Fact]
    public void Searcher_FindsStraightPath()
    {
        var searcher = new Searcher(Land.Empty(), new SearchCfg(0f, 0f));
        var path = searcher.Search(new int2(0,0), new int2(3,0));
        Assert.NotNull(path);
        Assert.Equal(new int2(0,0), path!.Start);
        Assert.Equal(new int2(3,0), path.End);
    }

    [Fact]
    public void Searcher_AvoidsHighCostTiles()
    {
        Func<int2, float> cost = pos => pos.Equals(new int2(1,0)) ? 100f : 0f;
        var searcher = new Searcher(
            Land.Empty(),
            new SearchCfg(0f, 0f),
            cost);

        var path = searcher.Search(new int2(0,0), new int2(2,0));

        Assert.NotNull(path);
        Assert.DoesNotContain(new int2(1,0), path!.Nodes);
    }

    [Fact]
    public void Searcher_RespectsNavGrid()
    {
        var nav = new NavGrid(new int2(3,1));
        nav.SetBlocked(new int2(1,0), true);
        var searcher = new Searcher(
            Land.Empty(),
            new SearchCfg(0f, 0f),
            navGrid: nav);

        var path = searcher.Search(new int2(0,0), new int2(2,0));

        Assert.NotNull(path);
        Assert.DoesNotContain(new int2(1,0), path!.Nodes);
    }
}
