using System;
using System.Collections.Generic;

namespace Unity.Entities {
    /// <summary>
    /// Minimal stand-alone ECS stubs so CoreEngine can operate without the
    /// real Unity.Entities package. Only basic functionality required by the
    /// ported systems is implemented.
    /// </summary>
    public struct Entity : IEquatable<Entity> {
        public int Index;
        public int Version;

        public bool Equals(Entity other) => Index == other.Index && Version == other.Version;
        public override bool Equals(object? obj) => obj is Entity e && Equals(e);
        public override int GetHashCode() => HashCode.Combine(Index, Version);
    }

    public interface IComponentData {}

    public class EntityManager {
        private int _next = 1;
        private readonly Dictionary<Entity, Dictionary<Type, object>> _components = new();

        /// <summary>Create a new entity.</summary>
        public Entity CreateEntity() {
            var e = new Entity { Index = _next++, Version = 1 };
            _components[e] = new Dictionary<Type, object>();
            return e;
        }

        /// <summary>Attach a component instance to an entity.</summary>
        public void AddComponentData<T>(Entity entity, T data) where T : struct {
            if (_components.TryGetValue(entity, out var comps))
                comps[typeof(T)] = data;
        }

        /// <summary>Check whether an entity currently has a component.</summary>
        public bool HasComponent<T>(Entity entity) where T : struct =>
            _components.TryGetValue(entity, out var comps) && comps.ContainsKey(typeof(T));

        /// <summary>Retrieve a component instance if present.</summary>
        public bool TryGetComponentData<T>(Entity entity, out T data) where T : struct {
            if (_components.TryGetValue(entity, out var comps) &&
                comps.TryGetValue(typeof(T), out var obj) && obj is T t) {
                data = t;
                return true;
            }
            data = default;
            return false;
        }
    }
}
