namespace VelorenPort.Server.Rtsim.Event;

/// <summary>
/// Event emitted every simulation tick.
/// </summary>
public readonly struct OnTick
{
    public readonly float Dt;
    public readonly ulong Tick;
    public readonly float WorldTime;

    public OnTick(float dt, ulong tick, float worldTime)
    {
        Dt = dt;
        Tick = tick;
        WorldTime = worldTime;
    }
}
