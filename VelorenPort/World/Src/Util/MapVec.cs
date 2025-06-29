using System.Collections.Generic;

namespace VelorenPort.World.Util;

/// <summary>
/// Deterministic dictionary with a default value, translated from Rust's
/// <c>MapVec</c>.
/// </summary>
public class MapVec<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _entries = new();
    private readonly TValue _default;

    public MapVec(TValue defaultValue)
    {
        _default = defaultValue;
    }

    public static MapVec<TKey, TValue> FromList(IEnumerable<(TKey Key, TValue Value)> entries, TValue defaultValue)
    {
        var map = new MapVec<TKey, TValue>(defaultValue);
        foreach (var (k, v) in entries)
            map._entries[k] = v;
        return map;
    }

    public TValue Get(TKey key) => _entries.TryGetValue(key, out var v) ? v : _default;

    public TValue GetOrCreate(TKey key)
    {
        if (!_entries.TryGetValue(key, out var v))
        {
            v = _default;
            _entries[key] = v;
        }
        return v;
    }

    public void Set(TKey key, TValue value) => _entries[key] = value;

    public IEnumerable<(TKey Key, TValue Value)> Items()
    {
        foreach (var kv in _entries)
            yield return (kv.Key, kv.Value);
    }

    public MapVec<TKey, TOut> Map<TOut>(System.Func<TKey, TValue, TOut> f, TOut defaultValue)
    {
        var result = new MapVec<TKey, TOut>(defaultValue);
        foreach (var kv in _entries)
            result.Set(kv.Key, f(kv.Key, kv.Value));
        return result;
    }

    public TValue this[TKey key]
    {
        get => Get(key);
        set => Set(key, value);
    }
}
