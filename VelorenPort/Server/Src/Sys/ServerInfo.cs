using System;
using VelorenPort.CoreEngine;
using VelorenPort.Server.Settings;

namespace VelorenPort.Server.Sys {
    /// <summary>
    /// Broadcasts basic server information to interested consumers every
    /// 60 ticks. This mirrors <c>server_info.rs</c> from the Rust project in a
    /// greatly simplified form.
    /// </summary>
    public record ServerInfo(uint GitHash, long GitTimestamp, ushort PlayersCount,
        ushort PlayerCap, BattleMode BattleMode);

    public static class GitInfo {
        public static readonly uint Hash;
        public static readonly long Timestamp;

        static GitInfo() {
            try {
                var psi = new System.Diagnostics.ProcessStartInfo("git", "rev-parse --short HEAD") {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                string? hash = proc?.StandardOutput.ReadLine();
                if (!string.IsNullOrWhiteSpace(hash))
                    Hash = Convert.ToUInt32(hash, 16);
            } catch { Hash = 0; }

            try {
                var psi = new System.Diagnostics.ProcessStartInfo("git", "log -1 --format=%ct") {
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                using var proc = System.Diagnostics.Process.Start(psi);
                string? ts = proc?.StandardOutput.ReadLine();
                if (!string.IsNullOrWhiteSpace(ts) && long.TryParse(ts, out var t))
                    Timestamp = t;
            } catch { Timestamp = 0; }
        }
    }

    public class ServerInfoBroadcaster {
        private readonly Action<ServerInfo> _send;
        private ulong _lastTick;

        public ServerInfoBroadcaster(Action<ServerInfo> send) {
            _send = send;
        }

        /// <summary>
        /// Call once per server tick with the current tick counter and
        /// player count. A message will be sent every 60 ticks.
        /// </summary>
        public void Update(ulong tick, Settings.Settings settings, int playersCount) {
            if (tick - _lastTick >= 60) {
                _lastTick = tick;
                var info = new ServerInfo(
                    GitInfo.Hash,
                    GitInfo.Timestamp,
                    (ushort)Math.Clamp(playersCount, 0, ushort.MaxValue),
                    (ushort)settings.MaxPlayers,
                    BattleMode.PvE);
                _send(info);
            }
        }
    }
}
