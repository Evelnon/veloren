using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Very small system that instantly teleports players when they
/// stand close to a teleporter. This mirrors the basic behaviour
/// of <c>server/src/sys/teleporter.rs</c> in a simplified form.
/// </summary>
public static class TeleporterSystem
{
    private const float Radius = 2f;
    private static readonly Dictionary<(Uid, Teleporter), DateTime> _cooldowns = new();

    public static void Update(IEnumerable<Client> clients, IEnumerable<Teleporter> teleporters)
    {
        var now = DateTime.UtcNow;
        foreach (var client in clients)
        {
            foreach (var tp in teleporters)
            {
                if (math.distance(client.Position.Value, tp.Position) <= Radius)
                {
                    var key = (client.Uid, tp);
                    if (!_cooldowns.TryGetValue(key, out var next) || next <= now)
                    {
                        client.SetPosition(tp.Target);
                        _cooldowns[key] = now + TimeSpan.FromSeconds(1);
                    }
                    break;
                }
            }
        }

        var expired = new List<(Uid, Teleporter)>();
        foreach (var (k, t) in _cooldowns)
            if (t <= now)
                expired.Add(k);
        foreach (var k in expired)
            _cooldowns.Remove(k);
    }
}
