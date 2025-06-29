using System.Linq;
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
    public void Searcher_AvoidsMapEdges()
    {
        var sim = new WorldSim(0, new int2(4, 4));
        var searcher = new Searcher(Land.FromSim(sim), new SearchCfg(0f, 0f, 5f));
        var path = searcher.Search(new int2(0, 0), new int2(0, 3));
        Assert.NotNull(path);
        Assert.True(path!.Nodes.Any(p => p.x > 0));
    }

    [Fact]
    public void Searcher_PrefersRoads_WhenDiscounted()
    {
        var sim = new WorldSim(0, new int2(3, 3));
        for (int y = 0; y < 3; y++)
        for (int x = 0; x < 3; x++)
        {
            var chunk = sim.Get(new int2(x, y));
            if (chunk == null) continue;
            chunk.Path = (new Sim.Way(), Sim.Path.Default);
        }

        int2[] road = { new int2(1,0), new int2(1,1), new int2(1,2), new int2(0,1), new int2(2,1) };
        foreach (var pos in road)
        {
            var c = sim.Get(pos)!;
            c.Path = (new Sim.Way { Neighbors = 1 }, Sim.Path.Default);
        }

        var searcherNoRoad = new Searcher(Land.FromSim(sim), new SearchCfg(0f, 0f));
        var straight = searcherNoRoad.Search(new int2(0,0), new int2(2,2));

        var searcherRoad = new Searcher(Land.FromSim(sim), new SearchCfg(0.75f, 0f));
        var roadPath = searcherRoad.Search(new int2(0,0), new int2(2,2));

        Assert.NotNull(straight);
        Assert.NotNull(roadPath);
        Assert.True(roadPath!.Nodes.Count > straight!.Nodes.Count);
        Assert.Contains(new int2(1,0), roadPath.Nodes);
    }
}
