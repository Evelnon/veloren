using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Simple container with stable integer identifiers. Mirrors <c>store.rs</c>.
    /// </summary>
    [Serializable]
    public class Store<T>
    {
        [Serializable]
        public readonly struct Id : IEquatable<Id>, IComparable<Id>
        {
            public readonly ulong Value;
            public Id(ulong value) => Value = value;
            public bool Equals(Id other) => Value == other.Value;
            public override bool Equals(object obj) => obj is Id other && Equals(other);
            public override int GetHashCode() => Value.GetHashCode();
            public int CompareTo(Id other) => Value.CompareTo(other.Value);
            public override string ToString() => $"Id<{typeof(T).Name}>({Value})";
        }

        private readonly List<T> _items = new();

        public IReadOnlyList<T> Items => _items;

        public T Get(Id id) => _items[(int)id.Value];
        public T this[Id id] => Get(id);

        public Id Insert(T item)
        {
            var id = new Id((ulong)_items.Count);
            _items.Add(item);
            return id;
        }

        public IEnumerable<Id> Ids()
        {
            for (ulong i = 0; i < (ulong)_items.Count; i++)
                yield return new Id(i);
        }

        public IEnumerable<T> Values() => _items;

        public IEnumerable<(Id id, T value)> Enumerate()
        {
            for (int i = 0; i < _items.Count; i++)
                yield return (new Id((ulong)i), _items[i]);
        }

        public Id? RecreateId(ulong value)
        {
            return value < (ulong)_items.Count ? new Id(value) : (Id?)null;
        }

        /// <summary>
        /// Iterate over items in parallel, returning their IDs and values.
        /// </summary>
        public IEnumerable<(Id id, T value)> ParEnumerate()
        {
            return _items.AsParallel().Select((item, index) => (new Id((ulong)index), item));
        }
    }
}
