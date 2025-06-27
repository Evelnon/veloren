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
    }

    /// <summary>Simple registry mapping commands to their metadata.</summary>
    public static class Cmd {
        private static readonly Dictionary<ServerChatCommand, ChatCommandData> _data = new() {
            { ServerChatCommand.Say, new ChatCommandData(new[]{"message"}, "Send a chat message", false) },
            { ServerChatCommand.Help, new ChatCommandData(Array.Empty<string>(), "List available commands", false) },
            { ServerChatCommand.Teleport, new ChatCommandData(new[]{"x","y","z"}, "Teleport to coordinates", true) },
        };

        public static ChatCommandData Data(ServerChatCommand cmd) => _data[cmd];
    }
}
