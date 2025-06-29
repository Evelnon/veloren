using VelorenPort.World;
using Unity.Mathematics;

namespace World.Tests;

public class WorldSimTests
{
    [Fact]
    public void ApproxChunkTerrainNormal_ReturnsNormalizedVector()
    {
        var sim = new WorldSim(123, new int2(8, 8));
        var normal = sim.ApproxChunkTerrainNormal(int2.zero);
        Assert.NotNull(normal);
        Assert.True(math.abs(math.length(normal!.Value) - 1f) < 1e-5f);
    }

    [Fact]
    public void GetSurfaceAltApprox_ConsidersWaterLevel()
    {
        var sim = new WorldSim(0, new int2(1, 1));
        var chunk = sim.Get(int2.zero)!;
        chunk.Alt = 5f;
        chunk.WaterAlt = 10f;
        sim.Set(int2.zero, chunk);

        float alt = sim.GetSurfaceAltApprox(int2.zero);
        Assert.Equal(10f, alt);
    }

    [Fact]
    public void Tick_PerformsErosion()
    {
        var sim = new WorldSim(0, new int2(3, 3));
        var chunk = sim.Get(int2.zero)!;
        float before = chunk.Alt;
        sim.Tick(1f);
        float after = sim.Get(int2.zero)!.Alt;
        Assert.NotEqual(before, after);
    public void Constructor_InitializesHumidityMap()
    {
        var sim = new WorldSim(0, new int2(2, 2));
        Assert.Equal(0.5f, sim.Humidity.Get(new int2(0, 0)));
    }

    [Fact]
    public void GenerateChunk_SetsHumidityMapValue()
    {
        var sim = new WorldSim(1, new int2(2, 2));
        var chunk = sim.Get(new int2(1, 1))!;
        Assert.Equal(chunk.Humidity, sim.Humidity.Get(new int2(1, 1)));
    }

    [Fact]
    public void Tick_DiffusesHumidity()
    {
        var sim = new WorldSim(2, new int2(2, 2));
        sim.Humidity.Set(new int2(0, 0), 1f);
        sim.Humidity.Set(new int2(1, 1), 0f);
        sim.Tick(0f);
        Assert.True(sim.Humidity.Get(new int2(1, 1)) > 0f);
    }
}

