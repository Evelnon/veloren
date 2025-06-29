using System.Collections.Generic;
using System.Linq;
using VelorenPort.World;
using VelorenPort.World.Civ;
using VelorenPort.NativeMath;

namespace World.Tests;

public class WorldIntegrationTests
{
    [Fact]
    public void World_GenerateAndTick_PersistsSitesRegionsAndLayers()
    {
        const uint seed = 123u;
        var (world, index) = World.Generate(seed);
        CivGenerator.Generate(world, index, 2);

        var siteSnapshot = index.Sites.Enumerate()
            .Select(p => (p.id.Value, p.value.Position, p.value.Name))
            .ToList();

        int2 chunkPos = new int2(1, 2);
        var (chunk, supplement) = index.Map.GetOrGenerateWithSupplement(chunkPos, world.Noise);
        int wildlifeCount = chunk.Wildlife.Count;
        int resourceCount = supplement.ResourceBlocks.Count;

        var region = world.Sim.Regions.Get(chunkPos);
        var regionSnapshot = region.Events.ToList();

        for (int i = 0; i < 5; i++)
            world.Tick(1f);

        var siteAfter = index.Sites.Enumerate()
            .Select(p => (p.id.Value, p.value.Position, p.value.Name))
            .ToList();
        Assert.Equal(siteSnapshot, siteAfter);

        var regionAfter = world.Sim.Regions.Get(chunkPos).Events.ToList();
        Assert.Equal(regionSnapshot, regionAfter);

        var (chunkAfter, supAfter) = index.Map.GetOrGenerateWithSupplement(chunkPos, world.Noise);
        Assert.Equal(wildlifeCount, chunkAfter.Wildlife.Count);
        Assert.Equal(resourceCount, supAfter.ResourceBlocks.Count);
    }
}
