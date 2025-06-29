using System.Linq;
using VelorenPort.World;
using VelorenPort.World.Civ;
using VelorenPort.NativeMath;

namespace World.Tests;

public class WorldStatePersistenceTests
{
    [Fact]
    public void World_Tick_PreservesSitesLayersWeatherEconomy()
    {
        const uint seed = 42u;
        var (world, index) = World.Generate(seed);
        CivGenerator.Generate(world, index, 2);

        var initialSites = index.Sites.Enumerate()
            .Select(p => (p.id.Value, p.value.Position, p.value.Name))
            .ToList();

        int2 chunkPos = new int2(0, 0);
        var (chunk, supplement) = index.Map.GetOrGenerateWithSupplement(chunkPos, world.Noise);
        int wildlifeBefore = chunk.Wildlife.Count;
        int entityBefore = supplement.Entities.Count;
        int resourceBefore = supplement.ResourceBlocks.Count;

        var weatherCellBefore = world.Sim.Weather.Grid.Get(new int2(0, 0));
        float econTimeBefore = index.EconomyContext.Time;

        for (int i = 0; i < 5; i++)
            world.Tick(1f);

        var sitesAfter = index.Sites.Enumerate()
            .Select(p => (p.id.Value, p.value.Position, p.value.Name))
            .ToList();
        Assert.Equal(initialSites, sitesAfter);

        var (chunkAfter, supAfter) = index.Map.GetOrGenerateWithSupplement(chunkPos, world.Noise);
        Assert.Equal(wildlifeBefore, chunkAfter.Wildlife.Count);
        Assert.Equal(entityBefore, supAfter.Entities.Count);
        Assert.Equal(resourceBefore, supAfter.ResourceBlocks.Count);

        var weatherCellAfter = world.Sim.Weather.Grid.Get(new int2(0, 0));
        Assert.NotEqual(weatherCellBefore.Cloud, weatherCellAfter.Cloud);
        Assert.Equal(econTimeBefore + 5f, index.EconomyContext.Time, 3);
    }
}
