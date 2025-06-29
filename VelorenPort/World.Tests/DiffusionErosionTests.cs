using VelorenPort.World;
using VelorenPort.World.Sim;
using VelorenPort.NativeMath;

namespace World.Tests;

public class DiffusionErosionTests
{
    [Fact]
    public void Diffusion_SmoothsAltitude()
    {
        var sim = new WorldSim(0, new int2(2, 1));
        var a = sim.Get(new int2(0, 0))!;
        var b = sim.Get(new int2(1, 0))!;
        a.Alt = 10f;
        b.Alt = 0f;
        sim.Set(new int2(0, 0), a);
        sim.Set(new int2(1, 0), b);

        Diffusion.Apply(sim, dt: 1f, kd: 0.5f);

        Assert.True(sim.Get(new int2(0, 0))!.Alt < 10f);
        Assert.True(sim.Get(new int2(1, 0))!.Alt > 0f);
    }

    [Fact]
    public void Erosion_LowersSteepSlope()
    {
        var sim = new WorldSim(0, new int2(2, 1));
        var a = sim.Get(new int2(0, 0))!;
        var b = sim.Get(new int2(1, 0))!;
        a.Alt = 10f;
        b.Alt = 0f;
        sim.Set(new int2(0, 0), a);
        sim.Set(new int2(1, 0), b);

        Erosion.Apply(sim, iterations: 1, k: 0.1f);

        Assert.True(sim.Get(new int2(0, 0))!.Alt < 10f);
    }

    [Fact]
    public void Erosion_ComputesFluxAndDownhill()
    {
        var sim = new WorldSim(0, new int2(2, 1));
        var a = sim.Get(new int2(0, 0))!;
        var b = sim.Get(new int2(1, 0))!;
        a.Alt = 10f;
        b.Alt = 0f;
        sim.Set(new int2(0, 0), a);
        sim.Set(new int2(1, 0), b);

        Erosion.Apply(sim, iterations: 1, k: 0.1f);

        var chunk = sim.Get(new int2(0, 0))!;
        Assert.NotNull(chunk.Downhill);
        Assert.True(chunk.Flux > 1f);
    }

    [Fact]
    public void Erosion_ReducesSlopeBetweenNeighbors()
    {
        var sim = new WorldSim(0, new int2(2, 1));
        var a = sim.Get(new int2(0, 0))!;
        var b = sim.Get(new int2(1, 0))!;
        a.Alt = 10f;
        b.Alt = 0f;
        sim.Set(new int2(0, 0), a);
        sim.Set(new int2(1, 0), b);

        float before = a.Alt - b.Alt;
        Erosion.Apply(sim, iterations: 10, k: 0.1f);
        float after = sim.Get(new int2(0, 0))!.Alt - sim.Get(new int2(1, 0))!.Alt;

        Assert.True(math.abs(after) < math.abs(before));
    }

    [Fact]
    public void River_CarvePaths_SetsVelocity()
    {
        var sim = new WorldSim(0, new int2(2, 1));
        var a = sim.Get(new int2(0, 0))!;
        var b = sim.Get(new int2(1, 0))!;
        a.Alt = 10f;
        b.Alt = 0f;
        sim.Set(new int2(0, 0), a);
        sim.Set(new int2(1, 0), b);

        Erosion.Apply(sim, iterations: 1, k: 0.1f);
        River.CarvePaths(sim, fluxThreshold: 1f);

        var chunk = sim.Get(new int2(0, 0))!;
        Assert.True(chunk.River.IsRiver);
        Assert.True(math.lengthsq(chunk.River.Velocity) > 0f);
    }

    [Fact]
    public void Diffusion_DoesNotDropBelowBasement()
    {
        var sim = new WorldSim(0, new int2(1, 1));
        var chunk = sim.Get(int2.zero)!;
        chunk.Alt = 5f;
        chunk.Basement = 4.5f;
        sim.Set(int2.zero, chunk);

        Diffusion.Apply(sim, dt: 1f, kd: 0.2f);

        Assert.True(sim.Get(int2.zero)!.Alt >= sim.Get(int2.zero)!.Basement);
    }
}
