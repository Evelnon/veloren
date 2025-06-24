using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Represents the presence of an entity in the world, controlling
    /// synchronization and view distances. Simplified version of the
    /// Presence component in Rust.
    /// </summary>
    [Serializable]
    public class Presence {
        public ViewDistance TerrainViewDistance { get; private set; }
        public ViewDistance EntityViewDistance { get; private set; }
        public PresenceKind Kind { get; set; }
        public bool LossyTerrainCompression { get; set; }

        public Presence(ViewDistances distances, PresenceKind kind)
            : this(distances, kind, DateTime.UtcNow) { }

        public Presence(ViewDistances distances, PresenceKind kind, DateTime now) {
            TerrainViewDistance = new ViewDistance(distances.Terrain, now);
            EntityViewDistance = new ViewDistance(distances.Entity, now);
            Kind = kind;
            LossyTerrainCompression = false;
        }

        public bool ControllingCharacter => Kind is PresenceKind.Character || Kind is PresenceKind.Possessor;

        public bool SyncMe => Kind is PresenceKind.Character || Kind is PresenceKind.Possessor;

        public CharacterId? CharacterId => Kind switch {
            PresenceKind.Character c => c.Id,
            PresenceKind.LoadingCharacter l => l.Id,
            _ => null,
        };
    }

    /// <summary>
    /// Different kinds of presence for a connected entity.
    /// </summary>
    [Serializable]
    public abstract record PresenceKind {
        [Serializable]
        public sealed record Spectator() : PresenceKind;

        [Serializable]
        public sealed record LoadingCharacter(CharacterId Id) : PresenceKind;

        [Serializable]
        public sealed record Character(CharacterId Id) : PresenceKind;

        [Serializable]
        public sealed record Possessor() : PresenceKind;
    }

    internal enum Direction {
        Up,
        Down,
    }

    /// <summary>
    /// Helper struct storing a view distance value that changes gradually.
    /// </summary>
    [Serializable]
    public struct ViewDistance {
        private Direction _direction;
        private DateTime _lastDirectionChange;
        private uint? _target;
        private uint _current;

        private static readonly TimeSpan TimePerDirChange = TimeSpan.FromMilliseconds(300);

        public ViewDistance(uint startValue, DateTime now) {
            _direction = Direction.Up;
            _lastDirectionChange = now - TimePerDirChange;
            _target = null;
            _current = startValue;
        }

        public uint Current => _current;

        public void Update(DateTime now) {
            if (_target.HasValue && now - _lastDirectionChange > TimePerDirChange) {
                _lastDirectionChange = now;
                _current = _target.Value;
                _target = null;
            }
        }

        public void SetTarget(uint newTarget, DateTime now) {
            Direction newDir = newTarget == _current ? _direction : newTarget < _current ? Direction.Down : Direction.Up;
            if (newDir == _direction) {
                _current = newTarget;
                _target = null;
            } else if (now - _lastDirectionChange > TimePerDirChange) {
                _direction = newDir;
                _lastDirectionChange = now;
                _current = newTarget;
                _target = null;
            } else {
                _target = newTarget;
            }
        }
    }
}
