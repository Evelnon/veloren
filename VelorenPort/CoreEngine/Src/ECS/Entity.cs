using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine.ECS
{
    /// <summary>
    /// Marker interface for component types.
    /// </summary>
    public interface IComponent { }

    /// <summary>
    /// Lightweight entity type similar to Specs' Entity.
    /// </summary>
    public struct Entity : IEquatable<Entity>
    {
        internal int Id { get; }
        internal Entity(int id) => Id = id;

        public bool Equals(Entity other) => Id == other.Id;
        public override bool Equals(object? obj) => obj is Entity other && Equals(other);
        public override int GetHashCode() => Id.GetHashCode();
        public override string ToString() => $"Entity({Id})";
    }

    /// <summary>
    /// Manages entity creation and component storage.
    /// Provides minimal API for Specs-like ECS.
    /// </summary>
    public class World
    {
        private int _nextId = 1;
        private readonly Dictionary<Entity, Dictionary<Type, object>> _components = new();

        public Entity CreateEntity()
        {
            var e = new Entity(_nextId++);
            _components[e] = new Dictionary<Type, object>();
            return e;
        }

        public bool Exists(Entity entity) => _components.ContainsKey(entity);

        public void Destroy(Entity entity) => _components.Remove(entity);

        public void Add<T>(Entity entity, T component)
        {
            if (_components.TryGetValue(entity, out var comps))
                comps[typeof(T)] = component!;
        }

        public bool Has<T>(Entity entity) =>
            _components.TryGetValue(entity, out var comps) && comps.ContainsKey(typeof(T));

        public bool TryGet<T>(Entity entity, out T component)
        {
            if (_components.TryGetValue(entity, out var comps) && comps.TryGetValue(typeof(T), out var obj) && obj is T value)
            {
                component = value;
                return true;
            }
            component = default!;
            return false;
        }

        public T Get<T>(Entity entity) where T : notnull => (T)_components[entity][typeof(T)];

        public void Set<T>(Entity entity, T component)
        {
            if (_components.TryGetValue(entity, out var comps))
                comps[typeof(T)] = component!;
        }

        public void Remove<T>(Entity entity)
        {
            if (_components.TryGetValue(entity, out var comps))
                comps.Remove(typeof(T));
        }

        public IEnumerable<Entity> EntitiesWith<T>()
        {
            foreach (var (entity, comps) in _components)
                if (comps.ContainsKey(typeof(T)))
                    yield return entity;
        }
    }

    /// <summary>
    /// Base type for systems executed with access to a world.
    /// </summary>
    public abstract class EcsSystem
    {
        public abstract void Run(World world);
    }

    /// <summary>
    /// Schedules a list of systems to run sequentially.
    /// </summary>
    public class Scheduler
    {
        private readonly List<EcsSystem> _systems = new();

        public void Add(EcsSystem system) => _systems.Add(system);

        public void Run(World world)
        {
            foreach (var system in _systems)
                system.Run(world);
        }
    }
}
