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

        public AdminList Admins { get; set; } = new();
        public Banlist Banlist { get; set; } = new();
        public Whitelist Whitelist { get; set; } = new();

        public static Settings Load(string dataDir) {
            var settingsPath = Path.Combine(dataDir, "settings.json");
            Settings settings;
            if (File.Exists(settingsPath)) {
                var json = File.ReadAllText(settingsPath);
                settings = JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            } else {
                settings = new Settings();
            }
            settings.Admins = AdminList.Load(Path.Combine(dataDir, "admins.json"));
            settings.Banlist = Banlist.Load(Path.Combine(dataDir, "banlist.json"));
            settings.Whitelist = Whitelist.Load(Path.Combine(dataDir, "whitelist.json"));
            return settings;
        }

        public void Save(string dataDir) {
            Directory.CreateDirectory(dataDir);
            var settingsPath = Path.Combine(dataDir, "settings.json");
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(settingsPath, json);
            Admins.Save(Path.Combine(dataDir, "admins.json"));
            Banlist.Save(Path.Combine(dataDir, "banlist.json"));
            Whitelist.Save(Path.Combine(dataDir, "whitelist.json"));
        }
    }
}
