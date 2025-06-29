using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine;

/// <summary>
/// Simplified 3D grid of volume chunks. Mirrors a subset of
/// common::volumes::vol_grid_3d from the Rust codebase.
/// </summary>
[Serializable]
public class VolGrid3d<T> where T : struct
{
    private readonly int3 _chunkSize;
    private readonly Dictionary<int3, Vol<T>> _chunks;
    private readonly T _default;

    private struct Int3Comparer : IEqualityComparer<int3>
    {
        public bool Equals(int3 a, int3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public int GetHashCode(int3 v) => HashCode.Combine(v.x, v.y, v.z);
    }

    public VolGrid3d(int3 chunkSize, T defaultValue)
    {
        _chunkSize = chunkSize;
        _default = defaultValue;
        _chunks = new Dictionary<int3, Vol<T>>(new Int3Comparer());
    }

    public int3 ChunkSize => _chunkSize;

    public static int3 ChunkKey(int3 pos, int3 chunkSize)
        => new int3(pos.x / chunkSize.x, pos.y / chunkSize.y, pos.z / chunkSize.z);

    public static int3 ChunkOffs(int3 pos, int3 chunkSize)
        => new int3(math.abs(pos.x % chunkSize.x), math.abs(pos.y % chunkSize.y), math.abs(pos.z % chunkSize.z));

    private Vol<T> GetOrCreateChunk(int3 key)
    {
        if (!_chunks.TryGetValue(key, out var chunk))
        {
            chunk = new Vol<T>(_chunkSize);
            chunk.Fill(_default);
            _chunks[key] = chunk;
        }
        return chunk;
    }

    public bool TryGetChunk(int3 key, out Vol<T> chunk) => _chunks.TryGetValue(key, out chunk);

    public T Get(int3 pos)
    {
        var key = ChunkKey(pos, _chunkSize);
        return TryGetChunk(key, out var chunk)
            ? chunk.Get(ChunkOffs(pos, _chunkSize))
            : _default;
    }

    public void Set(int3 pos, T value)
    {
        var key = ChunkKey(pos, _chunkSize);
        var chunk = GetOrCreateChunk(key);
        chunk.Set(ChunkOffs(pos, _chunkSize), value);
    }
}
