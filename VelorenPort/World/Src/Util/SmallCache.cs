using System;
using System.Collections.Generic;
using VelorenPort.World.Util;

namespace VelorenPort.World.Util
{
    /// <summary>
    /// Tiny fixed-size cache indexed by simple integer vectors.
    /// </summary>
    public class SmallCache<K, V> where K : struct, IEquatable<K>
    {
        private const int CacheLen = 32; // match Rust version
        private readonly K?[] _index = new K?[CacheLen + 9];
        private readonly V[] _data = new V[CacheLen + 9];
        private uint _random = 1;
        private readonly Func<K, IEnumerable<int>> _toInts;

        public SmallCache(Func<K, IEnumerable<int>> toInts)
        {
            _toInts = toInts;
        }

        private static int CalcIdx(IEnumerable<int> values)
        {
            uint r = 0;
            uint[] h = { 0x6eed0e9d, 0x2f72b421, 0x18132f72, 0x891e2fba };
            int i = 0;
            foreach (int e in values)
            {
                r ^= (uint)e * h[i];
                i++;
                if (i >= h.Length) break;
            }
            return (int)r;
        }

        private bool EqualsEntry(K? entry, K key)
        {
            return entry.HasValue && EqualityComparer<K>.Default.Equals(entry.Value, key);
        }

        public V Get(K key, Func<K, V> create)
        {
            int idx = CalcIdx(_toInts(key)) % CacheLen;

            if (EqualsEntry(_index[idx], key)) return _data[idx];
            if (EqualsEntry(_index[idx + 1], key)) return _data[idx + 1];
            if (EqualsEntry(_index[idx + 4], key)) return _data[idx + 4];
            if (EqualsEntry(_index[idx + 9], key)) return _data[idx + 9];

            for (int i = 0; i < 4; i++)
            {
                int pos = idx + i * i;
                if (!_index[pos].HasValue)
                {
                    _index[pos] = key;
                    _data[pos] = create(key);
                    return _data[pos];
                }
            }

            int step = (int)(SeedExpan.Diffuse(_random) % 4);
            int finalIdx = idx + step * step;
            _random++;
            _index[finalIdx] = key;
            _data[finalIdx] = create(key);
            return _data[finalIdx];
        }
    }
}
