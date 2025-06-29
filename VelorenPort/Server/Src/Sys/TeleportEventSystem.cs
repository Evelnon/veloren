using System.Collections.Generic;
using System.Linq;
using VelorenPort.Server.Events;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Applies <see cref="TeleportToPositionEvent"/>s emitted by other systems.
/// </summary>
public static class TeleportEventSystem
{
    public static void Update(EventManager events, IEnumerable<Client> clients)
    {
        var map = clients.ToDictionary(c => c.Uid);
        foreach (var ev in events.Drain<TeleportToPositionEvent>())
        {
            if (map.TryGetValue(ev.Entity, out var client))
                client.SetPosition(ev.Position);
        }
    }
}
