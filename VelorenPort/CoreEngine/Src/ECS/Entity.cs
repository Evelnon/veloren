using System;
using System.Collections;
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
        private readonly HashSet<Entity> _entities = new();
        private readonly Dictionary<Type, object> _components = new();

        private Dictionary<Entity, T> GetStorage<T>()
        {
            if (_components.TryGetValue(typeof(T), out var store))
                return (Dictionary<Entity, T>)store;

            var dict = new Dictionary<Entity, T>();
            _components[typeof(T)] = dict;
            return dict;
        }

        public Entity CreateEntity()
        {
            var e = new Entity(_nextId++);
            _entities.Add(e);
            return e;
        }

        public bool Exists(Entity entity) => _entities.Contains(entity);

        public void Destroy(Entity entity)
        {
            _entities.Remove(entity);
            foreach (var store in _components.Values)
                ((IDictionary)store).Remove(entity);
        }

        public void Add<T>(Entity entity, T component)
        {
            if (!_entities.Contains(entity))
                return;
            var store = GetStorage<T>();
            store[entity] = component!;
        }

        public bool Has<T>(Entity entity)
        {
            if (!_entities.Contains(entity))
                return false;
            return GetStorage<T>().ContainsKey(entity);
        }

        public bool TryGet<T>(Entity entity, out T component)
        {
            if (_entities.Contains(entity) && GetStorage<T>().TryGetValue(entity, out var value))
            {
                component = value;
                return true;
            }
            component = default!;
            return false;
        }

        public T Get<T>(Entity entity) where T : notnull => GetStorage<T>()[entity];

        public void Set<T>(Entity entity, T component)
        {
            if (_entities.Contains(entity))
                GetStorage<T>()[entity] = component!;
        }

        public void Remove<T>(Entity entity)
        {
            if (_entities.Contains(entity))
                GetStorage<T>().Remove(entity);
        }

        public IEnumerable<Entity> EntitiesWith<T>()
        {
            if (!_components.TryGetValue(typeof(T), out var storeObj))
                yield break;
            var store = (Dictionary<Entity, T>)storeObj;
            foreach (var e in store.Keys)
                yield return e;
        }

        public Query<T> Query<T>() => new Query<T>(GetStorage<T>());

        public Query<T1, T2> Query<T1, T2>() => new Query<T1, T2>(GetStorage<T1>(), GetStorage<T2>());
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

    /// <summary>
    /// Query over one component type.
    /// </summary>
    public readonly struct Query<T> : IEnumerable<(Entity, T)>
    {
        private readonly Dictionary<Entity, T> _store;

        internal Query(Dictionary<Entity, T> store) => _store = store;

        public Enumerator GetEnumerator() => new Enumerator(_store);

        IEnumerator<(Entity, T)> IEnumerable<(Entity, T)>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<(Entity, T)>
        {
            private Dictionary<Entity, T>.Enumerator _enumerator;

            internal Enumerator(Dictionary<Entity, T> store) => _enumerator = store.GetEnumerator();

            public (Entity, T) Current => (_enumerator.Current.Key, _enumerator.Current.Value);
            object IEnumerator.Current => Current;
            public bool MoveNext() => _enumerator.MoveNext();
            public void Reset() => throw new NotSupportedException();
            public void Dispose() => _enumerator.Dispose();
        }
    }

    /// <summary>
    /// Query over two component types.
    /// </summary>
    public readonly struct Query<T1, T2> : IEnumerable<(Entity, T1, T2)>
    {
        private readonly Dictionary<Entity, T1> _s1;
        private readonly Dictionary<Entity, T2> _s2;

        internal Query(Dictionary<Entity, T1> s1, Dictionary<Entity, T2> s2)
        {
            _s1 = s1;
            _s2 = s2;
        }

        public Enumerator GetEnumerator() => new Enumerator(_s1, _s2);

        IEnumerator<(Entity, T1, T2)> IEnumerable<(Entity, T1, T2)>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<(Entity, T1, T2)>
        {
            private Dictionary<Entity, T1>.Enumerator _e1;
            private readonly Dictionary<Entity, T2> _s2;
            private (Entity, T1, T2) _current;

            internal Enumerator(Dictionary<Entity, T1> s1, Dictionary<Entity, T2> s2)
            {
                _e1 = s1.GetEnumerator();
                _s2 = s2;
                _current = default;
            }

            public (Entity, T1, T2) Current => _current;
            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                while (_e1.MoveNext())
                {
                    var ent = _e1.Current.Key;
                    var c1 = _e1.Current.Value;
                    if (_s2.TryGetValue(ent, out var c2))
                    {
                        _current = (ent, c1, c2);
                        return true;
                    }
                }
                return false;
            }

            public void Reset() => throw new NotSupportedException();
            public void Dispose() => _e1.Dispose();
        }
    }
}
