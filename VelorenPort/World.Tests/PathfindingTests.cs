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
}
