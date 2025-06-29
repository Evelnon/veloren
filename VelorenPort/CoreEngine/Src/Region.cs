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
            EnqueueHistory(ev);
        }

        private void EnqueueHistory(RegionEvent ev)
        {
            _history.Enqueue(ev);
            while (_history.Count > HistoryLimit)
                _history.Dequeue();
        }

        /// <summary>
        /// Persist the current history queue to a file so that region events can
        /// survive server restarts. The format is a simple CSV per line.
        /// </summary>
        public void SaveHistory(string path)
        {
            using var writer = new System.IO.StreamWriter(path);
            foreach (var ev in _history)
            {
                switch (ev)
                {
                    case RegionEvent.Entered ent:
                        var from = ent.From.HasValue ? $"{ent.From.Value.x},{ent.From.Value.y}" : "_";
                        writer.WriteLine($"E,{ent.Entity.Value},{from}");
                        break;
                    case RegionEvent.Left left:
                        var to = left.To.HasValue ? $"{left.To.Value.x},{left.To.Value.y}" : "_";
                        writer.WriteLine($"L,{left.Entity.Value},{to}");
                        break;
                }
            }
        }

        /// <summary>
        /// Load previously saved history from <paramref name="path"/>. Any
        /// existing history entries are cleared.
        /// </summary>
        public void LoadHistory(string path)
        {
            if (!System.IO.File.Exists(path)) return;
            _history.Clear();
            foreach (var line in System.IO.File.ReadAllLines(path))
            {
                var parts = line.Split(',');
                if (parts.Length < 3) continue;
                if (!ulong.TryParse(parts[1], out var uid)) continue;
                int2? pos = null;
                if (parts[2] != "_")
                {
                    if (parts.Length >= 4 && int.TryParse(parts[2], out var px) && int.TryParse(parts[3], out var py))
                        pos = new int2(px, py);
                }
                if (parts[0] == "E")
                    EnqueueHistory(new RegionEvent.Entered(new Uid(uid), pos));
                else if (parts[0] == "L")
                    EnqueueHistory(new RegionEvent.Left(new Uid(uid), pos));
            }
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
    }
}
