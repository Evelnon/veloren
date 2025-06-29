using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        private readonly string _savePath;

        public CharacterUpdater(string? savePath = null) {
            _savePath = savePath ?? Path.Combine(DataDir.DefaultDataDirName, "characters.json");
        }

        public IReadOnlyDictionary<CharacterId, (string Player, string Alias, Body Body)> Characters => _characters;

        /// <summary>
        /// Stores a new character entry. Components are ignored for now but
        /// kept in the signature to remain compatible with the caller.
        /// </summary>
        public CharacterId CreateCharacter(Entity entity, string playerUuid, string alias, object components) {
            var id = new CharacterId(_nextId++);
            _characters[id] = (playerUuid, alias, Body.Humanoid);
            Console.WriteLine($"[CharacterUpdater] Created '{alias}' for {playerUuid} ({id.Value})");
            return id;
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

        /// <summary>
        /// Persist all known characters to disk. The format is a simple JSON
        /// dictionary keyed by character id.
        /// </summary>
        public void SaveAll() {
            var serializable = new Dictionary<long, object>();
            foreach (var (id, info) in _characters) {
                serializable[id.Value] = new {
                    info.Player,
                    info.Alias,
                    Body = (int)info.Body
                };
            }
            Directory.CreateDirectory(Path.GetDirectoryName(_savePath)!);
            var json = JsonSerializer.Serialize(serializable, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_savePath, json);
        }

        public void LoadAll() {
            if (!File.Exists(_savePath))
                return;

            var json = File.ReadAllText(_savePath);
            var data = JsonSerializer.Deserialize<Dictionary<long, SavedCharacter>>(json);
            if (data == null)
                return;

            _characters.Clear();
            _nextId = 1;
            foreach (var kv in data) {
                var id = new CharacterId(kv.Key);
                _characters[id] = (kv.Value.Player, kv.Value.Alias, (Body)kv.Value.Body);
                if (id.Value >= _nextId)
                    _nextId = id.Value + 1;
            }
        }

        private class SavedCharacter {
            public string Player { get; set; } = string.Empty;
            public string Alias { get; set; } = string.Empty;
            public int Body { get; set; }
        }
    }
}
