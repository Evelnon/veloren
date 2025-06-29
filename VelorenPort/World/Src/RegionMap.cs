using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Mathematics;

namespace VelorenPort.World
{
    /// <summary>
    /// Collection of regions keyed by chunk position. This mirrors a subset of
    /// the functionality provided by the Rust <c>RegionMap</c>.
    /// </summary>
    public class RegionMap
    {
        private readonly Dictionary<int2, Region> _regions = new();

        /// <summary>Retrieve an existing region or create a new one.</summary>
        public Region Get(int2 chunkPos)
        {
            if (!_regions.TryGetValue(chunkPos, out var region))
            {
                region = new Region(chunkPos);
                _regions[chunkPos] = region;
            }
            return region;
        }

        /// <summary>Remove a region from the map.</summary>
        public bool Remove(int2 chunkPos) => _regions.Remove(chunkPos);

        /// <summary>Advance simulation for all regions.</summary>
        public void Tick()
        {
            foreach (var region in _regions.Values)
                region.Tick();
        }

        /// <summary>Enumerate all stored regions.</summary>
        public IEnumerable<Region> All => _regions.Values;

        /// <summary>Remove all regions from the map.</summary>
        public void Clear() => _regions.Clear();

        /// <summary>
        /// Replace the current data with the contents of <paramref name="path"/>.
        /// </summary>
        public void LoadInto(string path)
        {
            Clear();
            var loaded = LoadFromFile(path);
            foreach (var (pos, region) in loaded._regions)
                _regions[pos] = region;
        }

        /// <summary>Persist this map to a JSON file.</summary>
        public void SaveToFile(string path)
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var data = new List<(int2 pos, Region region)>();
            foreach (var kv in _regions)
                data.Add((kv.Key, kv.Value));
            File.WriteAllText(path, JsonSerializer.Serialize(data, opts));
        }

        /// <summary>Load a map from a JSON file.</summary>
        public static RegionMap LoadFromFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);
            var map = new RegionMap();
            var data = JsonSerializer.Deserialize<List<(int2 pos, Region region)>>(File.ReadAllText(path));
            if (data != null)
            {
                foreach (var (pos, region) in data)
                    map._regions[pos] = region;
            }
            return map;
        }
    }
}
