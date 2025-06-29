using System.Linq;
using System.Collections.Generic;
using VelorenPort.World;
using VelorenPort.NativeMath;
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

    [Fact]
    public void Searcher_AvoidsHighAltitude_WhenWeighted()
    {
        var sim = new WorldSim(0, new int2(3,3));
        var high = sim.Get(new int2(1,1))!;
        high.Alt = 50f;
        sim.Set(new int2(1,1), high);

        var searcher = new Searcher(
            Land.FromSim(sim),
            new SearchCfg(0f, 0f, 0f, 1f));

        var path = searcher.Search(new int2(0,1), new int2(2,1));

        Assert.NotNull(path);
        Assert.DoesNotContain(new int2(1,1), path!.Nodes);
    }

    [Fact]
    public void Searcher_AvoidsResource_WhenCostPositive()
    {
        var sim = new WorldSim(0, new int2(3,3));
        sim.Nature.SetChunkResources(new int2(1,1), new Dictionary<ChunkResource, float>
        {
            [ChunkResource.Ore] = 1f
        });

        var costs = new Dictionary<ChunkResource, float> { [ChunkResource.Ore] = 100f };
        var searcher = new Searcher(
            Land.FromSim(sim),
            new SearchCfg(0f, 0f, 0f, 0f, costs));

        var path = searcher.Search(new int2(0,1), new int2(2,1));

        Assert.NotNull(path);
        Assert.DoesNotContain(new int2(1,1), path!.Nodes);
    }

    [Fact]
    public void Searcher_SeeksResource_WhenCostNegative()
    {
        var sim = new WorldSim(0, new int2(3,3));
        sim.Nature.SetChunkResources(new int2(1,1), new Dictionary<ChunkResource, float>
        {
            [ChunkResource.Ore] = 1f
        });

        var costs = new Dictionary<ChunkResource, float> { [ChunkResource.Ore] = -3f };
        var searcher = new Searcher(
            Land.FromSim(sim),
            new SearchCfg(0f, 0f, 0f, 0f, costs));

        var path = searcher.Search(new int2(0,0), new int2(2,0));

        Assert.NotNull(path);
        Assert.Contains(new int2(1,1), path!.Nodes);
    }

    [Fact]
    public void Searcher_UsesNavMesh_ToAvoidObstacles()
    {
        var vox = new bool[3,3,2];
        for (int x = 0; x < 3; x++)
        for (int y = 0; y < 3; y++)
        for (int z = 0; z < 2; z++)
            vox[x,y,z] = true;
        vox[1,0,0] = false;
        vox[1,1,0] = false;
        var mesh = NavMesh.Generate(vox);

        var searcher = new Searcher(
            Land.Empty(),
            new SearchCfg(0f, 0f),
            navMesh: mesh);

        var path = searcher.Search(new int2(0,0), new int2(2,2));

        Assert.NotNull(path);
        Assert.DoesNotContain(new int2(1,0), path!.Nodes);
        Assert.DoesNotContain(new int2(1,1), path!.Nodes);
    }

    [Fact]
    public void BuildNavMesh_FlagsWaterAsBlocked()
    {
        var sim = new WorldSim(0, new int2(3,3));
        var chunk = sim.Get(new int2(1,1))!;
        chunk.WaterAlt = chunk.Alt + 10f;
        sim.Set(new int2(1,1), chunk);

        var mesh = Pathfinding.BuildNavMesh(sim);
        var searcher = new Searcher(
            Land.FromSim(sim),
            new SearchCfg(0f, 0f),
            navMesh: mesh);

        var path = searcher.Search(new int2(0,1), new int2(2,1));

        Assert.NotNull(path);
        Assert.DoesNotContain(new int2(1,1), path!.Nodes);
    }
}
