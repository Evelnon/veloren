using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VelorenPort.Server.Settings {
    /// <summary>
    /// Simple whitelist store mirroring <c>settings/whitelist.rs</c>.
    /// </summary>
    public class Whitelist {
        public record WhitelistRecord(Guid Uuid, DateTime Date);

        private readonly Dictionary<Guid, WhitelistRecord> _entries = new();
        public IReadOnlyDictionary<Guid, WhitelistRecord> Entries => _entries;

        public void Add(Guid uuid) {
            _entries[uuid] = new WhitelistRecord(uuid, DateTime.UtcNow);
        }

        public void Remove(Guid uuid) => _entries.Remove(uuid);

        public bool Contains(Guid uuid) => _entries.ContainsKey(uuid);

        public static Whitelist Load(string path) {
            if (!File.Exists(path))
                return new Whitelist();
            try {
                var json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<WhitelistRecord>>(json);
                var wl = new Whitelist();
                if (list != null) {
                    foreach (var r in list)
                        wl._entries[r.Uuid] = r;
                }
                return wl;
            } catch {
                return new Whitelist();
            }
        }

        public void Save(string path) {
            var data = new List<WhitelistRecord>(_entries.Values);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
