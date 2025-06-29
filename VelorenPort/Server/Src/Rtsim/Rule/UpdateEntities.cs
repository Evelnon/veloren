using Unity.Mathematics;

namespace VelorenPort.Server.Rtsim.Rule;

/// <summary>
/// Simple rule that advances simulated entities based on their velocity.
/// </summary>
public class UpdateEntities : IRtsimRule
{
    public void OnBlockChange(Event.OnBlockChange ev, RtSim sim) {}

    public void OnTick(Event.OnTick ev, RtSim sim)
    {
        foreach (var entity in sim.Entities)
        {
            entity.Pos += entity.Velocity * ev.Dt;
        }
    }
}
