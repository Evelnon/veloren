using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Basic region tracking structure mirroring a very small portion of the
    /// Rust <c>region</c> module. Entities are referenced by <see cref="Uid"/>.
    /// </summary>
    [Serializable]
    public class Region {
        private readonly HashSet<Uid> _entities = new();
        private readonly List<RegionEvent> _events = new();
        private readonly Queue<RegionEvent> _history = new();
        private int _inactiveTicks = 0;

        private const int HistoryLimit = 64;

        public IReadOnlyCollection<Uid> Entities => _entities;
        public IReadOnlyList<RegionEvent> Events => _events;

        public void Add(Uid id, int2? from) {
            if (_entities.Add(id))
                PushEvent(new RegionEvent.Entered(id, from));
        }

        public void Remove(Uid id, int2? to) {
            if (_entities.Remove(id))
                PushEvent(new RegionEvent.Left(id, to));
        }

        public void Tick() {
            if (_events.Count > 0 || _entities.Count > 0)
                _inactiveTicks = 0;
            else
                _inactiveTicks++;

            _events.Clear();
        }

        public int InactiveTicks => _inactiveTicks;
        public bool Removable =>
            _entities.Count == 0 && _inactiveTicks > RegionMap.InactiveThreshold;

        public IReadOnlyCollection<RegionEvent> History => _history;

        private void PushEvent(RegionEvent ev)
        {
            _events.Add(ev);
            _history.Enqueue(ev);
            while (_history.Count > HistoryLimit)
                _history.Dequeue();
        }
    }

    /// <summary>Event raised when an entity changes regions.</summary>
    [Serializable]
    public abstract record RegionEvent {
        public sealed record Left(Uid Entity, int2? To) : RegionEvent;
        public sealed record Entered(Uid Entity, int2? From) : RegionEvent;
    }

    /// <summary>Map of regions keyed by chunk coordinates.</summary>
    [Serializable]
    public class RegionMap {
        private readonly Dictionary<int2, Region> _regions = new();
        private readonly Dictionary<Uid, int2> _entityRegion = new();

        public Region Get(int2 key) => _regions.TryGetValue(key, out var r) ? r : _regions[key] = new Region();

        public void Set(Uid id, int2 key) {
            if (_entityRegion.TryGetValue(id, out var oldKey) && oldKey.Equals(key)) return;
            Remove(id);
            _entityRegion[id] = key;
            Get(key).Add(id, oldKey);
        }

        public void Remove(Uid id) {
            if (_entityRegion.TryGetValue(id, out var key)) {
                if (_regions.TryGetValue(key, out var r)) r.Remove(id, null);
                _entityRegion.Remove(id);
            }
        }

        public const int InactiveThreshold = 60;

        public void Tick() {
            var toRemove = new List<int2>();
            foreach (var (key, region) in _regions) {
                region.Tick();
                if (region.Removable)
                    toRemove.Add(key);
            }
            foreach (var k in toRemove)
                _regions.Remove(k);
        }

        /// <summary>
        /// Retrieve events that occurred in the current tick for
        /// the region at <paramref name="key"/>. Returns an empty
        /// list if no region exists.
        /// </summary>
        public IReadOnlyList<RegionEvent> GetEvents(int2 key)
            => _regions.TryGetValue(key, out var r)
                ? r.Events
                : Array.Empty<RegionEvent>();

        /// <summary>
        /// Retrieve the stored history of events for the region at
        /// <paramref name="key"/>. Returns an empty collection if the
        /// region has not been created yet.
        /// </summary>
        public IReadOnlyCollection<RegionEvent> GetHistory(int2 key)
            => _regions.TryGetValue(key, out var r)
                ? r.History
                : Array.Empty<RegionEvent>();
    }
}
