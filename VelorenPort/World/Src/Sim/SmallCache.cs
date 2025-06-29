using System;
using System.Collections.Generic;

namespace VelorenPort.World.Sim;

/// <summary>
/// Minimal cache structure roughly mirroring the behaviour of
/// world/src/util/small_cache.rs. Entries are replaced at random when the
/// capacity is exceeded.
/// </summary>
public class SmallCache<K, V>
{
    private readonly int capacity;
    private readonly Dictionary<K, V> data = new();
    private readonly Random rng = new(1);

    public SmallCache(int capacity = 32)
    {
        this.capacity = capacity;
    }

    public V Get(K key, Func<K, V> factory)
    {
        if (data.TryGetValue(key, out var value))
            return value;

        if (data.Count >= capacity)
        {
            // Remove a random entry
            var idx = rng.Next(data.Count);
            var remKey = new List<K>(data.Keys)[idx];
            data.Remove(remKey);
        }

        value = factory(key);
        data[key] = value;
        return value;
    }
}
