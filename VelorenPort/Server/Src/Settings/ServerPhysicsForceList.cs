using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VelorenPort.Server.Settings {
    /// <summary>
    /// List of players forced to use server authoritative physics. This is a
    /// simplified port of <c>server_physics.rs</c>.
    /// </summary>
    public class ServerPhysicsForceList {
        public record ServerPhysicsForceRecord(Guid? By, string? Reason);

        private readonly Dictionary<Guid, ServerPhysicsForceRecord> _records = new();
        public IReadOnlyDictionary<Guid, ServerPhysicsForceRecord> Records => _records;

        public void Force(Guid uuid, ServerPhysicsForceRecord record) => _records[uuid] = record;
        public void Remove(Guid uuid) => _records.Remove(uuid);
        public bool Contains(Guid uuid) => _records.ContainsKey(uuid);

        public static ServerPhysicsForceList Load(string path) {
            if (!File.Exists(path))
                return new ServerPhysicsForceList();
            try {
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<Dictionary<Guid, ServerPhysicsForceRecord>>(json);
                var list = new ServerPhysicsForceList();
                if (data != null)
                    foreach (var (k, v) in data)
                        list._records[k] = v;
                return list;
            } catch {
                return new ServerPhysicsForceList();
            }
        }

        public void Save(string path) {
            var json = JsonSerializer.Serialize(_records, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
