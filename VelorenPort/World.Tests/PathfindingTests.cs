using VelorenPort.World;
using Unity.Mathematics;

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
    public void Searcher_AvoidsHighCost()
    {
        float Cost(int2 pos) => pos.Equals(new int2(1, 0)) ? 100f : 0f;
        var cfg = new SearchCfg(0f, 0f, Cost);
        var searcher = new Searcher(Land.Empty(), cfg);
        var path = searcher.Search(new int2(0, 0), new int2(2, 0));
        Assert.NotNull(path);
        Assert.Equal(new[] { new int2(0,0), new int2(0,1), new int2(2,1), new int2(2,0) }, path!.Nodes);
    }
}
