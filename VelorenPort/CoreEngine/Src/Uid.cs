using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine.ECS;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public struct Uid : IEquatable<Uid> {
        public ulong Value;
        public Uid(ulong value) { Value = value; }
        public bool Equals(Uid other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Uid other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
        public static implicit operator ulong(Uid id) => id.Value;
        public static implicit operator Uid(ulong value) => new Uid(value);
    }

    internal class UidAllocator {
        private ulong _nextUid = 0;
        public Uid Allocate() => new Uid(_nextUid++);
    }

    /// <summary>
    /// Mapping from various IDs to ECS entities.
    /// </summary>
    public class IdMaps {
        private readonly Dictionary<Uid, Entity> _uidMapping = new();
        private readonly Dictionary<CharacterId, Entity> _characterToEcs = new();
        private readonly Dictionary<RtSimEntity, Entity> _rtsimToEcs = new();
        private readonly UidAllocator _allocator = new();

        public Entity? GetEntity(Uid uid) => _uidMapping.TryGetValue(uid, out var e) ? e : (Entity?)null;
        public Entity? GetEntity(CharacterId id) => _characterToEcs.TryGetValue(id, out var e) ? e : (Entity?)null;
        public Entity? GetEntity(RtSimEntity id) => _rtsimToEcs.TryGetValue(id, out var e) ? e : (Entity?)null;

        public Entity? GetEntity(Actor actor) => actor switch {
            Actor.Character c => GetEntity(c.Id),
            Actor.Npc n => GetEntity(new RtSimEntity(n.Id)),
            _ => null
        };

        public void AddEntity(Uid uid, Entity entity) => _uidMapping[uid] = entity;
        public void AddCharacter(CharacterId id, Entity entity) => _characterToEcs[id] = entity;
        public void AddRtSim(RtSimEntity id, Entity entity) => _rtsimToEcs[id] = entity;

        public Uid Allocate(Entity entity) {
            var uid = _allocator.Allocate();
            _uidMapping[uid] = entity;
            return uid;
        }

        public void RemapEntity(Uid uid, Entity newEntity) => _uidMapping[uid] = newEntity;

        public Entity? RemoveEntity(Uid? uid = null, CharacterId? cid = null, RtSimEntity? rid = null) {
            Entity? found = null;
            if (uid.HasValue && _uidMapping.TryGetValue(uid.Value, out var e)) {
                _uidMapping.Remove(uid.Value);
                found = e;
            }
            if (cid.HasValue && _characterToEcs.TryGetValue(cid.Value, out var ce)) {
                _characterToEcs.Remove(cid.Value);
                found ??= ce;
            }
            if (rid.HasValue && _rtsimToEcs.TryGetValue(rid.Value, out var re)) {
                _rtsimToEcs.Remove(rid.Value);
                found ??= re;
            }
            return found;
        }
    }
}
