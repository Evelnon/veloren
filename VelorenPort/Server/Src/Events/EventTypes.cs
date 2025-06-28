using System;

namespace VelorenPort.Server.Events {
    /// <summary>
    /// Minimal list of events used by the server. The Rust code exposes many
    /// more variants but for now we only implement a small subset.
    /// </summary>
    public enum EventType {
        ClientDisconnect,
        ChatMessage,
        Command,
        CreateNpc,
        Explosion,
        InventoryManip,
        Respawn,
        Shoot,
        Mount,
        UpdateCharacter,
    }
}
