namespace VelorenPort.Server.Rtsim;

/// <summary>
/// Interface for basic rtsim rules.
/// </summary>
public interface IRtsimRule
{
    void OnBlockChange(Event.OnBlockChange ev, RtSim sim);
    void OnTick(Event.OnTick ev, RtSim sim);
}
