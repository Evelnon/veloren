using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Very small subset of the Rust <c>cmd</c> module providing
    /// basic chat command data and a couple of example commands.
    /// </summary>
    [Serializable]
    public class ChatCommandData {
        public string[] Args { get; }
        public string Description { get; }
        public bool NeedsAdmin { get; }

        public ChatCommandData(string[] args, string description, bool needsAdmin) {
            Args = args;
            Description = description;
            NeedsAdmin = needsAdmin;
        }
    }

    /// <summary>Enumeration of a few server chat commands.</summary>
    public enum ServerChatCommand {
        Say,
        Help,
        Teleport,
        Online,
        Invite,
        AcceptInvite,
        DeclineInvite,
        SetWaypoint,
        Whisper,
        Team,
    }

    /// <summary>Simple registry mapping commands to their metadata.</summary>
    public static class Cmd {
        private static readonly Dictionary<ServerChatCommand, ChatCommandData> _data = new() {
            { ServerChatCommand.Say, new ChatCommandData(new[]{"message"}, "Send a chat message", false) },
            { ServerChatCommand.Help, new ChatCommandData(Array.Empty<string>(), "List available commands", false) },
            { ServerChatCommand.Teleport, new ChatCommandData(new[]{"x","y","z"}, "Teleport to coordinates", true) },
            { ServerChatCommand.Online, new ChatCommandData(Array.Empty<string>(), "List online players", false) },
            { ServerChatCommand.Invite, new ChatCommandData(new[]{"uid","kind"}, "Invite a player to group or trade", false) },
            { ServerChatCommand.AcceptInvite, new ChatCommandData(new[]{"uid","kind"}, "Accept a pending invite", false) },
            { ServerChatCommand.DeclineInvite, new ChatCommandData(new[]{"uid","kind"}, "Decline a pending invite", false) },
            { ServerChatCommand.SetWaypoint, new ChatCommandData(Array.Empty<string>(), "Set a personal waypoint", false) },
            { ServerChatCommand.Whisper, new ChatCommandData(new[]{"uid","message"}, "Send a private message", false) },
            { ServerChatCommand.Team, new ChatCommandData(new[]{"message"}, "Send a message to your group", false) },
        };

        public static ChatCommandData Data(ServerChatCommand cmd) => _data[cmd];
    }
}
