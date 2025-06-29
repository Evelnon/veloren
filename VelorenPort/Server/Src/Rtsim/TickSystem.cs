using VelorenPort.Server.Ecs;

namespace VelorenPort.Server.Rtsim;

/// <summary>
/// Simple dispatcher system that advances the rtsim each frame.
/// </summary>
public class TickSystem : IGameSystem
{
    private readonly RtSim _sim;

    public TickSystem(RtSim sim)
    {
        _sim = sim;
    }

    public void Update(float dt, Events.EventManager events)
    {
        _sim.Tick(dt);
    }
}
