using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server {
    /// <summary>
    /// Shared server constants and utility types.
    /// Mirrors definitions from <c>server/src/lib.rs</c> that are
    /// required by multiple systems.
    /// </summary>
    public static class Lib {
        /// <summary>
        /// Seconds a player must wait before switching battle mode again.
        /// </summary>
        public const double BattleModeCooldown = 60.0 * 5.0;

        /// <summary>Minimum view distance kept loaded around each player.</summary>
        public const uint MinViewDistance = 6;
    }

    /// <summary>Default spawn point used when a player has no waypoint.</summary>
    public readonly struct SpawnPoint {
        public readonly float3 Position;
        public SpawnPoint(float3 position) { Position = position; }
        public static SpawnPoint Default => new SpawnPoint(new float3(0, 0, 256));
    }

    /// <summary>Tick counter used for throttling network updates.</summary>
    public readonly struct Tick {
        public readonly ulong Value;
        public Tick(ulong value) { Value = value; }
        public static implicit operator ulong(Tick t) => t.Value;
    }

    /// <summary>Records the start time of a tick for metrics.</summary>
    public readonly struct TickStart {
        public readonly DateTime Instant;
        public TickStart(DateTime instant) { Instant = instant; }
    }

    /// <summary>
    /// Stores battle mode cooldowns for players while offline.
    /// </summary>
    public class BattleModeBuffer {
        private readonly Dictionary<CharacterId, (BattleMode Mode, Time Time)> _map = new();

        public void Push(CharacterId id, (BattleMode Mode, Time Time) save) => _map[id] = save;

        public (BattleMode Mode, Time Time)? Get(CharacterId id) =>
            _map.TryGetValue(id, out var v) ? v : null;

        public (BattleMode Mode, Time Time)? Pop(CharacterId id) {
            if (_map.TryGetValue(id, out var v)) {
                _map.Remove(id);
                return v;
            }
            return null;
        }
    }
}
