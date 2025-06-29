using VelorenPort.Server.Rtsim;
using VelorenPort.Server.Rtsim.Rule;

namespace Server.Tests;

public class ResourceSimulationTests
{
    [Fact]
    public void Tick_ReplenishesResources()
    {
        var sim = new RtSim { ResourceCounter = 5, MaxResources = 20 };
        sim.AddRule(new DepleteResources());
        sim.AddRule(new ReplenishResources());

        sim.Emit(new VelorenPort.Server.Rtsim.Event.OnBlockChange(3));
        sim.Tick(0f);
        Assert.Equal(2, sim.ResourceCounter);

        sim.Tick(5f); // replenish
        Assert.True(sim.ResourceCounter > 2);
    }
}
