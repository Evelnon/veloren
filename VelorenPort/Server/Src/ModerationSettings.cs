using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace VelorenPort.Server {
    /// <summary>
    /// Settings controlling automated moderation features. Mirrors
    /// <c>ModerationSettings</c> from the Rust server implementation.
    /// </summary>
    [Serializable]
    public class ModerationSettings {
        public List<string> BannedWordsFiles { get; set; } = new();
        public bool Automod { get; set; } = false;
        public bool AdminsExempt { get; set; } = true;

        /// <summary>
        /// Load banned words from the configured files within the server_config
        /// directory located under <paramref name="dataDir"/>.
        /// </summary>
        public List<string> LoadBannedWords(string dataDir) {
            var banned = new List<string>();
            foreach (var file in BannedWordsFiles) {
                var path = Path.Combine(dataDir, "server_config", file);
                try {
                    if (File.Exists(path)) {
                        var json = File.ReadAllText(path);
                        var words = JsonSerializer.Deserialize<List<string>>(json);
                        if (words != null) banned.AddRange(words);
                    }
                } catch {
                    // Ignore invalid files just like the Rust version logs errors
                }
            }
            return banned;
        }
    }
}
