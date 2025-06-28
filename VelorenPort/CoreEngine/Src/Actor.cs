using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Union type representing either a NPC or a character.
    /// Mirrors the Actor enum in the Rust code which stores the corresponding id.
    /// </summary>
    [Serializable]
    public abstract record Actor {
        public sealed record Npc(NpcId Id) : Actor;
        public sealed record Character(CharacterId Id) : Actor;

        public static implicit operator Actor(NpcId npcId) => new Npc(npcId);
        public static implicit operator Actor(CharacterId id) => new Character(id);
    }
}
