using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VelorenPort.Server.Settings {
    /// <summary>
    /// Description and rules presented to connecting players. This mirrors the
    /// functionality of <c>server_description.rs</c> but without versioned
    /// migrations.
    /// </summary>
    public class ServerDescriptions {
        public string DefaultLocale { get; set; } = "en";
        public Dictionary<string, ServerDescription> Descriptions { get; set; } = new();

        public ServerDescription Get(string? locale) {
            var key = locale != null && Descriptions.ContainsKey(locale)
                ? locale
                : DefaultLocale;
            return Descriptions.TryGetValue(key, out var desc)
                ? desc
                : new ServerDescription();
        }

        public string? GetRules(string? locale) {
            var key = locale != null && Descriptions.ContainsKey(locale)
                ? locale
                : DefaultLocale;
            if (Descriptions.TryGetValue(key, out var desc) && desc.Rules != null)
                return desc.Rules;
            return Descriptions.TryGetValue(DefaultLocale, out var def) ? def.Rules : null;
        }

        public static ServerDescriptions Load(string path) {
            if (!File.Exists(path))
                return new ServerDescriptions { Descriptions = { ["en"] = new ServerDescription() } };
            try {
                var json = File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<ServerDescriptions>(json);
                if (data != null && data.Descriptions.Count > 0)
                    return data;
            } catch { }
            // return defaults on failure
            return new ServerDescriptions { Descriptions = { ["en"] = new ServerDescription() } };
        }

        public void Save(string path) {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }

    public class ServerDescription {
        public string Motd { get; set; } = "This is the best Veloren server";
        public string? Rules { get; set; }
    }
}
