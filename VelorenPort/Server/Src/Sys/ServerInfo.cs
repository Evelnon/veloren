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
        // TODO: Populate these from build metadata
        public const uint Hash = 0;
        public const long Timestamp = 0;
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
