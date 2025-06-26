using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;

#nullable enable

namespace VelorenPort.CLI {
    /// <summary>
    /// Configuration settings for the command line utilities. Mirrors the
    /// behaviour of <c>settings.rs</c> in the original project.
    /// </summary>
    [Serializable]
    public class Settings {
        public uint UpdateShutdownGracePeriodSecs { get; set; }
        public string UpdateShutdownMessage { get; set; } = string.Empty;
        public IPEndPoint WebAddress { get; set; } = new IPEndPoint(IPAddress.Loopback, 14005);
        public string? WebChatSecret { get; set; }
        public string? UiApiSecret { get; set; }
        public List<ShutdownSignal> ShutdownSignals { get; set; } = new();

        public Settings() {
            UpdateShutdownGracePeriodSecs = 120;
            UpdateShutdownMessage = "The server is restarting for an update";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                ShutdownSignals.Add(ShutdownSignal.SIGUSR1);
            }
        }

        public static Settings Load() {
            string path = GetSettingsPath();
            if (File.Exists(path)) {
                try {
                    string json = File.ReadAllText(path);
                    var loaded = System.Text.Json.JsonSerializer.Deserialize<Settings>(json);
                    if (loaded != null) return loaded;
                } catch (Exception) {
                    try {
                        string newPath = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, "settings.invalid.json");
                        File.Move(path, newPath, true);
                    } catch { }
                }
            }
            var defaults = new Settings();
            defaults.Save();
            return defaults;
        }

        private void Save() {
            try {
                string dir = Path.GetDirectoryName(GetSettingsPath()) ?? string.Empty;
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(GetSettingsPath(), json);
            } catch { }
        }

        public static string GetSettingsPath() {
            return Path.Combine(DataDir(), "settings.json");
        }

        public static string DataDir() {
            var baseDir = VelorenPort.CoreEngine.UserdataDir.Workspace();
            return Path.Combine(baseDir.FullName, "server-cli");
        }
    }

    public enum ShutdownSignal {
        SIGUSR1,
        SIGUSR2,
        SIGTERM,
    }

    public static class ShutdownSignalExtensions {
        public static int ToSignal(this ShutdownSignal sig) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                return sig switch {
                    ShutdownSignal.SIGUSR1 => 10,
                    ShutdownSignal.SIGUSR2 => 12,
                    ShutdownSignal.SIGTERM => 15,
                    _ => 0,
                };
            }
            throw new PlatformNotSupportedException();
        }
    }
}
