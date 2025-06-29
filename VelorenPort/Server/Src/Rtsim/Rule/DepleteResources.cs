namespace VelorenPort.Server.Rtsim.Rule;

/// <summary>
/// Very small rule that decreases the simulation's resource counter whenever
/// blocks are changed.
/// </summary>
public class DepleteResources : IRtsimRule
{
    public void OnBlockChange(Event.OnBlockChange ev, RtSim sim)
    {
        sim.ResourceCounter -= ev.Count;
    }

    public void OnTick(Event.OnTick ev, RtSim sim)
    {
        // no-op
    }
}
