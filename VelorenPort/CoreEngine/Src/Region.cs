using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelorenPort.NativeMath;

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
        private readonly int _historyLimit;
        private bool _modified = false;
        private int _inactiveTicks = 0;

        public Region(int historyLimit = 64)
        {
            _historyLimit = historyLimit;
        }

        public IReadOnlyCollection<Uid> Entities => _entities;
        public IReadOnlyList<RegionEvent> Events => _events;
        public int HistoryLimit => _historyLimit;

        public void Add(Uid id, int2? from) {
            if (_entities.Add(id)) {
                PushEvent(new RegionEvent.Entered(id, from));
                _modified = true;
            }
        }

        public void Remove(Uid id, int2? to) {
            if (_entities.Remove(id)) {
                PushEvent(new RegionEvent.Left(id, to));
                _modified = true;
            }
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

        public bool Modified
        {
            get => _modified;
            set => _modified = value;
        }

        public IReadOnlyCollection<RegionEvent> History => _history;

        private void PushEvent(RegionEvent ev)
        {
            _events.Add(ev);
            EnqueueHistory(ev);
        }

        private void EnqueueHistory(RegionEvent ev)
        {
            _history.Enqueue(ev);
            while (_history.Count > _historyLimit)
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

        public void Save(string path)
        {
            var opts = new JsonSerializerOptions { WriteIndented = false };
            File.WriteAllText(path, JsonSerializer.Serialize(this, opts));
            _modified = false;
        }

        public static Region Load(string path)
        {
            var opts = new JsonSerializerOptions { WriteIndented = false };
            return JsonSerializer.Deserialize<Region>(File.ReadAllText(path), opts) ?? new Region();
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
        private readonly string? _path;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            IncludeFields = true,
            WriteIndented = false
        };

        public RegionMap(string? path = null)
        {
            _path = path;
            if (_path != null && Directory.Exists(_path))
                LoadAll();
        }

        private string FilePath(int2 key) =>
            Path.Combine(_path!, $"region_{key.x}_{key.y}.json");

        private void LoadAll()
        {
            foreach (var file in Directory.GetFiles(_path!, "region_*.json"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var parts = name.Split('_');
                if (parts.Length < 3) continue;
                if (int.TryParse(parts[1], out var x) && int.TryParse(parts[2], out var y))
                {
                    var key = new int2(x, y);
                    var region = Region.Load(file);
                    _regions[key] = region;
                    foreach (var id in region.Entities)
                        _entityRegion[id] = key;
                }
            }
        }

        public Region Get(int2 key)
        {
            if (_regions.TryGetValue(key, out var r)) return r;
            if (_path != null)
            {
                var file = FilePath(key);
                if (File.Exists(file))
                {
                    r = Region.Load(file);
                    _regions[key] = r;
                    foreach (var id in r.Entities)
                        _entityRegion[id] = key;
                    return r;
                }
            }
            return _regions[key] = new Region();
        }

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
                if (_path != null && region.Modified) {
                    region.Save(FilePath(key));
                }
                if (region.Removable)
                    toRemove.Add(key);
            }
            foreach (var k in toRemove)
            {
                if (_path != null && _regions.TryGetValue(k, out var r))
                    r.Save(FilePath(k));
                _regions.Remove(k);
            }
        }

        public void Flush()
        {
            if (_path == null) return;
            foreach (var (key, region) in _regions)
                region.Save(FilePath(key));
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
