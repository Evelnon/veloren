using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Teleports players when they remain within range of a portal object
/// for the required buildup time. This mirrors a small portion of
/// `sys/object.rs` in the Rust server.
/// </summary>
public static class PortalSystem
{
    private const float RadiusSq = 4f; // 2 units
    private static readonly Dictionary<(Entity Portal, Uid Player), float> Timers = new();

    public static void Update(EntityManager em, IEnumerable<Client> clients, float dt)
    {
        foreach (var portal in em.GetEntitiesWith<Object>())
        {
            var obj = em.GetComponentData<Object>(portal);
            if (obj.Kind != ObjectKind.Portal)
                continue;
            if (!em.TryGetComponentData(portal, out Pos pos))
                continue;

            foreach (var c in clients)
            {
                var key = (portal, c.Uid);
                if (math.distancesq(pos.Value, c.Position.Value) <= RadiusSq)
                {
                    Timers.TryGetValue(key, out var t);
                    t += dt;
                    Timers[key] = t;
                    if (t >= obj.BuildupTime)
                    {
                        c.SetPosition(obj.Target);
                        Timers.Remove(key);
                    }
                }
                else
                {
                    Timers.Remove(key);
                }
            }
        }
    }
}
