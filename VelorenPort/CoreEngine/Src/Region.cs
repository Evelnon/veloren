using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            IncludeFields = true,
            WriteIndented = false
        };

        public Region Get(int2 key) => _regions.TryGetValue(key, out var r) ? r : _regions[key] = new Region();

        public int2? GetRegion(Uid id) => _entityRegion.TryGetValue(id, out var pos) ? pos : (int2?)null;

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

        /// <summary>Serialize all regions to JSON at <paramref name="path"/>.</summary>
        public void Save(string path)
        {
            var dto = new RegionMapDto();
            foreach (var (pos, region) in _regions)
                dto.Regions[$"{pos.x},{pos.y}"] = region;
            File.WriteAllText(path, JsonSerializer.Serialize(dto, JsonOpts));
        }

        /// <summary>Load regions from <paramref name="path"/> replacing current state.</summary>
        public static RegionMap Load(string path)
        {
            if (!File.Exists(path)) return new RegionMap();
            var dto = JsonSerializer.Deserialize<RegionMapDto>(File.ReadAllText(path), JsonOpts) ?? new RegionMapDto();
            var map = new RegionMap();
            foreach (var (key, region) in dto.Regions)
            {
                var parts = key.Split(',');
                var pos = new int2(int.Parse(parts[0]), int.Parse(parts[1]));
                map._regions[pos] = region;
                foreach (var id in region.Entities)
                    map._entityRegion[id] = pos;
            }
            return map;
        }

        private class RegionMapDto
        {
            public Dictionary<string, Region> Regions { get; set; } = new();
        }
    }
}
