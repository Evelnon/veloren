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
        public void AddComponentData<T>(Entity entity, T data) {
            if (_components.TryGetValue(entity, out var comps))
                comps[typeof(T)] = data;
        }

        /// <summary>Check whether an entity currently has a component.</summary>
        public bool HasComponent<T>(Entity entity) =>
            _components.TryGetValue(entity, out var comps) && comps.ContainsKey(typeof(T));

        /// <summary>Retrieve a component instance if present.</summary>
        public bool TryGetComponentData<T>(Entity entity, out T data) {
            if (_components.TryGetValue(entity, out var comps) &&
                comps.TryGetValue(typeof(T), out var obj) && obj is T t) {
                data = t;
                return true;
            }
            data = default;
            return false;
        }

        /// <summary>Directly get a component or throw if missing.</summary>
        public T GetComponentData<T>(Entity entity) =>
            TryGetComponentData<T>(entity, out var data)
                ? data
                : throw new InvalidOperationException("Component not found");

        /// <summary>Replace an existing component instance.</summary>
        public void SetComponentData<T>(Entity entity, T data)
        {
            if (_components.TryGetValue(entity, out var comps))
                comps[typeof(T)] = data;
        }

        /// <summary>Remove a component from an entity.</summary>
        public void RemoveComponent<T>(Entity entity)
        {
            if (_components.TryGetValue(entity, out var comps))
                comps.Remove(typeof(T));
        }

        /// <summary>Destroy an entity and all of its components.</summary>
        public void DestroyEntity(Entity entity) => _components.Remove(entity);

        /// <summary>Check if an entity still exists.</summary>
        public bool Exists(Entity entity) => _components.ContainsKey(entity);

        /// <summary>Enumerate all entities that have the specified component.</summary>
        public IEnumerable<Entity> GetEntitiesWith<T>()
        {
            foreach (var (ent, comps) in _components)
                if (comps.ContainsKey(typeof(T)))
                    yield return ent;
        }
    }
}
