using System.Collections.Generic;
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

    public static void Update(IEnumerable<Client> clients, IEnumerable<Teleporter> teleporters)
    {
        foreach (var client in clients)
        {
            foreach (var tp in teleporters)
            {
                if (math.distance(client.Position.Value, tp.Position) <= Radius)
                {
                    client.SetPosition(tp.Target);
                    break;
                }
            }
        }
    }
}
