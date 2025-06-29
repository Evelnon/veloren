using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine;

/// <summary>
/// Simplified 2D grid of volume chunks. This mirrors a fraction of
/// `common::volumes::vol_grid_2d` from the Rust codebase but is reduced
/// to the basic storage and access logic required for early world generation.
/// </summary>
[Serializable]
public class VolGrid2d<T> where T : struct
{
    private readonly int2 _mapSize;
    private readonly int3 _chunkSize;
    private readonly Dictionary<int2, Vol<T>> _chunks;
    private readonly T _default;

    private struct Int2Comparer : IEqualityComparer<int2>
    {
        public bool Equals(int2 a, int2 b) => a.x == b.x && a.y == b.y;
        public int GetHashCode(int2 v) => HashCode.Combine(v.x, v.y);
    }

    public VolGrid2d(int2 mapSize, int3 chunkSize, T defaultValue)
    {
        _mapSize = mapSize;
        _chunkSize = chunkSize;
        _default = defaultValue;
        _chunks = new Dictionary<int2, Vol<T>>(new Int2Comparer());
    }

    public int2 MapSize => _mapSize;
    public int3 ChunkSize => _chunkSize;

    public static int2 ChunkKey(int3 pos, int3 chunkSize)
        => new int2(pos.x / chunkSize.x, pos.y / chunkSize.y);

    public static int3 KeyChunk(int2 key, int3 chunkSize)
        => new int3(key.x * chunkSize.x, key.y * chunkSize.y, 0);

    public static int3 ChunkOffs(int3 pos, int3 chunkSize)
        => new int3(math.abs(pos.x % chunkSize.x), math.abs(pos.y % chunkSize.y), pos.z);

    private Vol<T> GetOrCreateChunk(int2 key)
    {
        if (!_chunks.TryGetValue(key, out var chunk))
        {
            chunk = new Vol<T>(_chunkSize);
            chunk.Fill(_default);
            _chunks[key] = chunk;
        }
        return chunk;
    }

    public bool TryGetChunk(int2 key, out Vol<T> chunk) => _chunks.TryGetValue(key, out chunk);

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
