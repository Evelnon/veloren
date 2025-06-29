using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.Server;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Evaluates wiring elements and propagates their outputs across circuits.
/// This is a greatly simplified version of <c>server/src/sys/wiring.rs</c>.
/// </summary>
public static class WiringSystem
{
    public static void Update(EntityManager em)
    {
        // Build a map of all wiring elements present in the world
        var elementMap = new Dictionary<Entity, WiringElement>();
        foreach (var entity in em.GetEntitiesWith<WiringElement>())
            elementMap[entity] = em.GetComponentData<WiringElement>(entity);

        // Evaluate each element and capture its outputs
        var outputs = new Dictionary<Entity, Dictionary<string, float>>();
        foreach (var (ent, elem) in elementMap)
            outputs[ent] = elem.Evaluate(elementMap);

        // Propagate signals along circuits
        foreach (var circuitEntity in em.GetEntitiesWith<Circuit>())
        {
            var circuit = em.GetComponentData<Circuit>(circuitEntity);
            foreach (var wire in circuit.Wires)
            {
                if (!outputs.TryGetValue(wire.Input.Entity, out var map))
                    continue;
                map.TryGetValue(wire.Input.Name, out var val);
                if (elementMap.TryGetValue(wire.Output.Entity, out var target))
                {
                    target.Inputs[wire.Output.Name] = val;
                    em.SetComponentData(wire.Output.Entity, target);
                    elementMap[wire.Output.Entity] = target;
                }
            }
        }
    }
}
