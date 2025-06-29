using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.Server.Rtsim;

/// <summary>
/// Extremely trimmed down real time simulation used for tests. It holds a set of
/// rules and processes queued events every tick.
/// </summary>
public class RtSim
{
    private readonly List<IRtsimRule> _rules = new();
    private readonly List<Event.OnBlockChange> _blockEvents = new();
    private ulong _tick;

    public class SimEntity
    {
        public float3 Pos;
        public float3 Velocity;
    }

    public IList<SimEntity> Entities { get; } = new List<SimEntity>();

    public DateTime Started { get; } = DateTime.UtcNow;
    public float Time { get; private set; }
    public int ResourceCounter { get; set; }
    public int MaxResources { get; set; } = 1000;

    public void AddRule(IRtsimRule rule) => _rules.Add(rule);

    public void Emit(Event.OnBlockChange ev) => _blockEvents.Add(ev);

    public void Tick(float dt)
    {
        Time += dt;
        _tick++;
        var tickEvent = new Event.OnTick(dt, _tick, Time);
        foreach (var rule in _rules)
            rule.OnTick(tickEvent, this);

        foreach (var ev in _blockEvents)
            foreach (var rule in _rules)
                rule.OnBlockChange(ev, this);
        _blockEvents.Clear();
    }
}
