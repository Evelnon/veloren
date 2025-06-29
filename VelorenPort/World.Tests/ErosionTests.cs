using VelorenPort.World;
using VelorenPort.World.Sim;
using Unity.Mathematics;

namespace World.Tests;

public class ErosionTests
{
    [Fact]
    public void IterativeApply_LowersAltitude()
    {
        var sim = new WorldSim(0, new int2(2,2));
        // Initialize chunks with an artificial slope
        foreach (var pos in new[]{int2.zero, new int2(1,0), new int2(0,1), new int2(1,1)})
        {
            sim.Set(pos, new SimChunk { Alt = pos.x + pos.y });
        }

        float before = sim.Get(int2.zero)!.Alt;
        Erosion.IterativeApply(sim, 3);
        float after = sim.Get(int2.zero)!.Alt;
        Assert.True(after < before);
    }
}
