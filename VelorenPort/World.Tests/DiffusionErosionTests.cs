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
}
