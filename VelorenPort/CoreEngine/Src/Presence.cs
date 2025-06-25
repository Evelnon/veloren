using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Represents the presence of an entity in the world, controlling
    /// synchronization and view distances. Mirrors the <c>Presence</c>
    /// component from the Rust codebase.
    /// </summary>
    [Serializable]
    public class Presence {
        public ViewDistance TerrainViewDistance { get; private set; }
        public ViewDistance EntityViewDistance { get; private set; }
        public PresenceKind Kind { get; private set; }
        // Only expose the identifier once the character has finished loading
        // to mirror the Rust implementation where `LoadingCharacter` is not
        // yet associated with a valid mapping.
        public CharacterId? CharacterId => Kind switch {
            PresenceKind.Character c => c.Id,
            _ => null
        };
        public bool LossyTerrainCompression { get; set; }

        public Presence(ViewDistances distances, PresenceKind kind)
            : this(distances, kind, DateTime.UtcNow) { }

        public Presence(ViewDistances distances, PresenceKind kind, DateTime now) {
            TerrainViewDistance = new ViewDistance(distances.Terrain, now);
            EntityViewDistance = new ViewDistance(distances.Entity, now);
            Kind = kind;
            LossyTerrainCompression = false;
        }

        public bool ControllingCharacter => Kind is PresenceKind.Character or PresenceKind.Possessor;

        public bool SyncMe => Kind is PresenceKind.Character or PresenceKind.Possessor;

        public void SetKind(PresenceKind kind) {
            Kind = kind;
        }
    }

    /// <summary>
    /// Different kinds of presence for a connected entity.
    /// </summary>
    [Serializable]
    public abstract record PresenceKind {
        public sealed record Spectator : PresenceKind;
        public sealed record LoadingCharacter(CharacterId Id) : PresenceKind;
        public sealed record Character(CharacterId Id) : PresenceKind;
        public sealed record Possessor : PresenceKind;
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
