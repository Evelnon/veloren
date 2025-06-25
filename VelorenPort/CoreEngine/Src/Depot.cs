using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Generic high performance allocator similar to common/src/depot.rs
    /// </summary>
    [Serializable]
    public class Depot<T>
    {
        [Serializable]
        public struct Id : IEquatable<Id>
        {
            public uint Index { get; }
            public uint Generation { get; }
            public ulong FullId => (ulong)Index | ((ulong)Generation << 32);

            public Id(uint index, uint generation)
            {
                Index = index;
                Generation = generation;
            }

            public bool Equals(Id other) => Index == other.Index && Generation == other.Generation;
            public override bool Equals(object obj) => obj is Id other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(Index, Generation);
            public override string ToString() => $"Id<{typeof(T).Name}>({Index}, {Generation})";
        }

        private struct Entry
        {
            public uint Generation;
            public T? Item;
        }

        private readonly List<Entry> _entries = new();
        private int _len = 0;

        public bool IsEmpty => _len == 0;
        public int Count => _len;

        public bool Contains(Id id)
        {
            return id.Index < _entries.Count &&
                   _entries[(int)id.Index].Generation == id.Generation &&
                   _entries[(int)id.Index].Item != null;
        }

        public T? Get(Id id)
        {
            return Contains(id) ? _entries[(int)id.Index].Item : default;
        }

        public T? GetMut(Id id)
        {
            if (!Contains(id)) return default;
            return _entries[(int)id.Index].Item;
        }

        public IEnumerable<Id> Ids()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry.Item != null)
                    yield return new Id((uint)i, entry.Generation);
            }
        }

        public IEnumerable<T> Values()
        {
            foreach (var entry in _entries)
            {
                if (entry.Item != null)
                    yield return entry.Item;
            }
        }

        public IEnumerable<T> ValuesMut()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry.Item != null)
                    yield return entry.Item;
            }
        }

        public IEnumerable<(Id id, T value)> Enumerate()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry.Item != null)
                    yield return (new Id((uint)i, entry.Generation), entry.Item);
            }
        }

        public Id Insert(T item)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                if (entry.Item == null)
                {
                    entry.Item = item;
                    entry.Generation++;
                    _entries[i] = entry;
                    _len++;
                    return new Id((uint)i, entry.Generation);
                }
            }

            var newEntry = new Entry { Generation = 0, Item = item };
            _entries.Add(newEntry);
            _len++;
            return new Id((uint)(_entries.Count - 1), 0);
        }

        public T? Remove(Id id)
        {
            if (Contains(id))
            {
                var entry = _entries[(int)id.Index];
                T item = entry.Item!;
                entry.Item = default;
                _entries[(int)id.Index] = entry;
                _len--;
                return item;
            }
            return default;
        }

        public Id? RecreateId(ulong fullId)
        {
            if (fullId >= (ulong)_entries.Count)
                return null;
            int idx = (int)fullId;
            var entry = _entries[idx];
            return new Id((uint)idx, entry.Generation);
        }
    }
}
