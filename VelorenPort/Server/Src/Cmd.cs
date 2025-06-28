using System;
using System.Linq;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server;

/// <summary>
/// Utilities to parse and execute basic chat commands.
/// This mirrors a tiny subset of <c>server/src/cmd.rs</c>.
/// </summary>
public static class Cmd
{
    /// <summary>Attempt to parse a chat line as a command.</summary>
    public static bool TryParse(string text, out ServerChatCommand command, out string[] args)
    {
        command = default;
        args = Array.Empty<string>();
        if (string.IsNullOrWhiteSpace(text) || !text.StartsWith("/"))
            return false;
        var parts = text[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;
        if (!Enum.TryParse(parts[0], true, out command))
            return false;
        args = parts.Skip(1).ToArray();
        return true;
    }

    /// <summary>Execute a parsed command.</summary>
    public static string Execute(ServerChatCommand command, GameServer server, Client client, string[] args)
    {
        switch (command)
        {
            case ServerChatCommand.Say:
                var msg = string.Join(' ', args);
                server.NotifyPlayers(msg);
                return msg;
            case ServerChatCommand.Help:
                return string.Join(", ", Enum.GetNames(typeof(ServerChatCommand)));
            case ServerChatCommand.Teleport:
                if (args.Length >= 3 &&
                    float.TryParse(args[0], out var x) &&
                    float.TryParse(args[1], out var y) &&
                    float.TryParse(args[2], out var z))
                {
                    client.SetPosition(new float3(x, y, z));
                    return $"Teleported to ({x}, {y}, {z})";
                }
                return "Invalid coordinates";
            case ServerChatCommand.Online:
                return string.Join(", ", server.GetOnlinePlayerNames());
            default:
                return "Unknown command";
        }
    }
}
