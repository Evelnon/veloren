using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Settings {
    /// <summary>
    /// Stores administrator records loaded from disk. This is a simplified
    /// counterpart to <c>settings/admin.rs</c> from the Rust server.
    /// </summary>
    public class AdminList {
        public record AdminRecord(Guid Uuid, AdminRole Role, DateTime Date);

        private readonly Dictionary<Guid, AdminRecord> _admins = new();
        public IReadOnlyDictionary<Guid, AdminRecord> Admins => _admins;

        public void Grant(Guid uuid, AdminRole role) {
            _admins[uuid] = new AdminRecord(uuid, role, DateTime.UtcNow);
        }

        public void Revoke(Guid uuid) => _admins.Remove(uuid);

        public bool TryGet(Guid uuid, out AdminRecord record) => _admins.TryGetValue(uuid, out record);

        public static AdminList Load(string path) {
            if (!File.Exists(path))
                return new AdminList();
            try {
                var json = File.ReadAllText(path);
                var entries = JsonSerializer.Deserialize<List<AdminRecord>>(json);
                var list = new AdminList();
                if (entries != null) {
                    foreach (var r in entries)
                        list._admins[r.Uuid] = r;
                }
                return list;
            } catch {
                return new AdminList();
            }
        }

        public void Save(string path) {
            var data = new List<AdminRecord>(_admins.Values);
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
