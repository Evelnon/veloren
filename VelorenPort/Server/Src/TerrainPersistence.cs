using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using VelorenPort.World;

namespace VelorenPort.Server {
    /// <summary>
    /// Basic terrain persistence system. Stores modifications to terrain chunks
    /// and writes them to disk. This mirrors <c>server/src/terrain_persistence.rs</c>
    /// but omits the complex LRU cache and versioned format for brevity.
    /// </summary>
    public class TerrainPersistence : IDisposable {
        private readonly string _path;
        private readonly Dictionary<int2, LoadedChunk> _chunks = new();
        private readonly LruCache<int2, ChunkData> _cached = new(32);

        public TerrainPersistence(string dataDir) {
            _path = Path.Combine(dataDir, "terrain");
            Directory.CreateDirectory(_path);
        }

        /// <summary>Apply any persisted changes onto a newly generated chunk.</summary>
        public void ApplyChanges(int2 key, Chunk chunk) {
            var loaded = LoadChunk(key);
            foreach (var kv in loaded.Chunk.Blocks) {
                var rel = kv.Key;
                chunk[rel.x, rel.y, rel.z] = kv.Value;
            }
        }

        private DateTime _lastFlush = DateTime.UtcNow;
        /// <summary>Flush modified chunks to disk every few seconds.</summary>
        public void Maintain() {
            if ((DateTime.UtcNow - _lastFlush).TotalSeconds < 5)
                return;
            FlushModified();
            _lastFlush = DateTime.UtcNow;
        }

        public void FlushModified() {
            foreach (var (key, loaded) in _chunks) {
                if (loaded.Modified) {
                    SaveChunk(key, loaded.Chunk);
                    loaded.Modified = false;
                }
            }
        }

        public void SetBlock(int3 worldPos, Block block) {
            var key = new int2(
                math.floor((float)worldPos.x / Chunk.Size.x),
                math.floor((float)worldPos.y / Chunk.Size.y));
            var rel = new int3(
                worldPos.x - key.x * Chunk.Size.x,
                worldPos.y - key.y * Chunk.Size.y,
                worldPos.z);
            var loaded = LoadChunk(key);
            if (!loaded.Chunk.Blocks.TryGetValue(rel, out var existing) || !existing.Equals(block)) {
                loaded.Chunk.Blocks[rel] = block;
                loaded.Modified = true;
            }
        }

        public void UnloadChunk(int2 key) {
            if (_chunks.Remove(key, out var loaded)) {
                if (loaded.Modified) {
                    SaveChunk(key, loaded.Chunk);
                }
                _cached.Put(key, loaded.Chunk);
            }
        }

        public void ClearChunk(int2 key) {
            _cached.Remove(key);
            _chunks[key] = new LoadedChunk { Chunk = new ChunkData(), Modified = true };
        }

        public void UnloadAll() {
            foreach (var key in new List<int2>(_chunks.Keys))
                UnloadChunk(key);
        }

        private string PathFor(int2 key) =>
            System.IO.Path.Combine(_path, $"chunk_{key.x}_{key.y}.dat");

        private LoadedChunk LoadChunk(int2 key) {
            if (_chunks.TryGetValue(key, out var loaded))
                return loaded;

            if (_cached.TryGet(key, out var cached)) {
                loaded = new LoadedChunk { Chunk = cached, Modified = false };
                _chunks[key] = loaded;
                return loaded;
            }

            var path = PathFor(key);
            if (File.Exists(path)) {
                try {
                    using var br = new BinaryReader(File.OpenRead(path));
                    var chunk = ChunkData.Deserialize(br);
                    loaded = new LoadedChunk { Chunk = chunk, Modified = false };
                    _chunks[key] = loaded;
                    return loaded;
                } catch { }
            }

            loaded = new LoadedChunk { Chunk = new ChunkData(), Modified = false };
            _chunks[key] = loaded;
            return loaded;
        }

        private void SaveChunk(int2 key, ChunkData chunk) {
            var path = PathFor(key);
            using var bw = new BinaryWriter(File.Create(path));
            chunk.Serialize(bw);
        }

        public void Dispose() => UnloadAll();

        private class LoadedChunk {
            public ChunkData Chunk = new ChunkData();
            public bool Modified;
        }

        private class ChunkData {
            public Dictionary<int3, Block> Blocks { get; } = new();

            public void Serialize(BinaryWriter bw) {
                bw.Write(1u); // version
                bw.Write(Blocks.Count);
                foreach (var (pos, block) in Blocks) {
                    bw.Write(pos.x); bw.Write(pos.y); bw.Write(pos.z);
                    bw.Write(block.ToUInt32());
                }
            }

            public static ChunkData Deserialize(BinaryReader br) {
                var version = br.ReadUInt32();
                var count = br.ReadInt32();
                var data = new ChunkData();
                for (int i = 0; i < count; i++) {
                    var x = br.ReadInt32();
                    var y = br.ReadInt32();
                    var z = br.ReadInt32();
                    var block = Block.FromUInt32(br.ReadUInt32());
                    data.Blocks[new int3(x, y, z)] = block;
                }
                return data;
            }
        }

        /// <summary>Minimal LRU cache used for recently unloaded chunks.</summary>
        private class LruCache<TKey, TValue> where TKey : notnull {
            private readonly int _capacity;
            private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Val)>> _map = new();
            private readonly LinkedList<(TKey Key, TValue Val)> _list = new();

            public LruCache(int capacity) { _capacity = capacity; }

            public bool TryGet(TKey key, out TValue val) {
                if (_map.TryGetValue(key, out var node)) {
                    _list.Remove(node);
                    _list.AddFirst(node);
                    val = node.Value.Val;
                    return true;
                }
                val = default!;
                return false;
            }

            public void Put(TKey key, TValue val) {
                if (_map.TryGetValue(key, out var node)) {
                    _list.Remove(node);
                } else if (_list.Count >= _capacity) {
                    var last = _list.Last!;
                    _map.Remove(last.Value.Key);
                    _list.RemoveLast();
                }
                var newNode = new LinkedListNode<(TKey,TValue)>((key, val));
                _list.AddFirst(newNode);
                _map[key] = newNode;
            }

            public void Remove(TKey key) {
                if (_map.TryGetValue(key, out var node)) {
                    _list.Remove(node);
                    _map.Remove(key);
                }
            }
        }
    }
}
