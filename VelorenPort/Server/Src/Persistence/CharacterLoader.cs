using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Persistence {
    /// <summary>
    /// Simplified character loader that works with the in-memory
    /// <see cref="CharacterUpdater"/>. Database support is planned
    /// but not yet implemented.
    /// </summary>
    public class CharacterLoader {
        private readonly Dictionary<string, List<CharacterId>> _characters = new();
        private readonly string _savePath;

        public CharacterLoader(string? savePath = null) {
            _savePath = savePath ?? Path.Combine(DataDir.DefaultDataDirName, "character_list.json");
        }

        public IReadOnlyList<CharacterId> LoadCharacterList(string playerUuid) {
            return _characters.TryGetValue(playerUuid, out var list)
                ? list
                : (IReadOnlyList<CharacterId>)System.Array.Empty<CharacterId>();
        }

        public void AddCharacter(string playerUuid, CharacterId id) {
            if (!_characters.TryGetValue(playerUuid, out var list)) {
                list = new List<CharacterId>();
                _characters[playerUuid] = list;
            }
            list.Add(id);
        }

        public void SaveAll() {
            Directory.CreateDirectory(Path.GetDirectoryName(_savePath)!);
            var serializable = new Dictionary<string, List<long>>();
            foreach (var (player, chars) in _characters) {
                var ids = new List<long>();
                foreach (var c in chars)
                    ids.Add(c.Value);
                serializable[player] = ids;
            }
            var json = JsonSerializer.Serialize(serializable, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_savePath, json);
        }

        public void LoadAll() {
            if (!File.Exists(_savePath))
                return;
            var json = File.ReadAllText(_savePath);
            var data = JsonSerializer.Deserialize<Dictionary<string, List<long>>>(json);
            if (data == null)
                return;
            _characters.Clear();
            foreach (var (player, ids) in data) {
                var list = new List<CharacterId>();
                foreach (var id in ids)
                    list.Add(new CharacterId(id));
                _characters[player] = list;
            }
        }
    }
}
