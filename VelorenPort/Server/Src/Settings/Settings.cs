using System;
using System.IO;
using System.Text.Json;

namespace VelorenPort.Server.Settings {
    /// <summary>
    /// Basic server configuration loaded from a JSON file. This is a trimmed
    /// down version of the Rust settings module and is sufficient for testing.
    /// </summary>
    public class Settings {
        public string ServerName { get; set; } = "Veloren Server";
        public uint WorldSeed { get; set; } = 1;
        public int MaxPlayers { get; set; } = 100;
        public bool EnableQueryServer { get; set; } = false;
        public int QueryServerPort { get; set; } = 14006;
        public ushort QueryServerRatelimit { get; set; } = 120;

        public static Settings Load(string path) {
            if (File.Exists(path)) {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            return new Settings();
        }

        public void Save(string path) {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
