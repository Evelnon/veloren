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
}

