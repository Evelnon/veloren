namespace VelorenPort.Server.Rtsim.Rule;

/// <summary>
/// Replenish resources gradually every tick.
/// </summary>
public class ReplenishResources : IRtsimRule
{
    private const float RatePerSecond = 1f; // resources regenerated per second

    public void OnBlockChange(Event.OnBlockChange ev, RtSim sim) {}

    public void OnTick(Event.OnTick ev, RtSim sim)
    {
        if (sim.ResourceCounter < sim.MaxResources)
        {
            float add = RatePerSecond * ev.Dt;
            int inc = (int)add;
            if (inc > 0)
            {
                sim.ResourceCounter = System.Math.Min(sim.ResourceCounter + inc, sim.MaxResources);
            }
        }
    }
}
