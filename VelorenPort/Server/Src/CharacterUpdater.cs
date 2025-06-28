using System;
using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;

namespace VelorenPort.Server {
    /// <summary>
    /// Very small in-memory character persistence used while the full
    /// database-backed updater is ported. The implementation keeps a simple
    /// dictionary of created characters and allows basic edits.
    /// </summary>
    public class CharacterUpdater {
        private readonly Dictionary<CharacterId, (string Player, string Alias, Body Body)> _characters = new();
        private long _nextId = 1;

        /// <summary>
        /// Stores a new character entry. Components are ignored for now but
        /// kept in the signature to remain compatible with the caller.
        /// </summary>
        public void CreateCharacter(Entity entity, string playerUuid, string alias, object components) {
            var id = new CharacterId(_nextId++);
            _characters[id] = (playerUuid, alias, Body.Humanoid);
            Console.WriteLine($"[CharacterUpdater] Created '{alias}' for {playerUuid} ({id.Value})");
        }

        /// <summary>
        /// Updates the alias and body of an existing character if present.
        /// </summary>
        public void EditCharacter(Entity entity, string playerUuid, CharacterId id, string alias, Body body) {
            if (_characters.ContainsKey(id)) {
                _characters[id] = (playerUuid, alias, body);
                Console.WriteLine($"[CharacterUpdater] Edited character {id.Value}");
            }
        }
    }
}
