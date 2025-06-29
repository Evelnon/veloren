using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Persistence {
    /// <summary>
    /// Character list persistence backed by SQLite. Older JSON files are
    /// automatically imported on first run.
    /// </summary>
    public class CharacterLoader {
        private readonly Database _db;
        private readonly string _legacyPath;

        public CharacterLoader(string? dbPath = null) {
            var path = dbPath ?? Path.Combine(DataDir.DefaultDataDirName, "characters.sqlite");
            _legacyPath = Path.Combine(Path.GetDirectoryName(path)!, "character_list.json");
            _db = new Database(path);
            MigrateLegacy();
        }

        private void MigrateLegacy() {
            if (!File.Exists(_legacyPath)) return;
            var json = File.ReadAllText(_legacyPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, List<long>>>(json);
            if (data == null) return;
            foreach (var (player, ids) in data) {
                foreach (var id in ids) {
                    AddCharacter(player, new CharacterId(id), "legacy");
                }
            }
            File.Move(_legacyPath, _legacyPath + ".bak", true);
        }

        public IReadOnlyList<CharacterId> LoadCharacterList(string playerUuid) {
            var list = new List<CharacterId>();
            using var cmd = _db.Connection.CreateCommand();
            cmd.CommandText = "SELECT id FROM character WHERE player_uuid = $p";
            cmd.Parameters.AddWithValue("$p", playerUuid);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) list.Add(new CharacterId(reader.GetInt64(0)));
            return list;
        }

        public void AddCharacter(string playerUuid, CharacterId id, string alias) {
            using var cmd = _db.Connection.CreateCommand();
            cmd.CommandText = "INSERT INTO character(id, player_uuid, alias) VALUES($id,$player,$alias)";
            cmd.Parameters.AddWithValue("$id", id.Value);
            cmd.Parameters.AddWithValue("$player", playerUuid);
            cmd.Parameters.AddWithValue("$alias", alias);
            cmd.ExecuteNonQuery();
        }

        public void SaveAll() { }
        public void LoadAll() { }
    }
}
