using System;
using System.Collections.Generic;
using System.Linq;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Weighted random item selector mirroring a subset of the Rust
    /// <c>Lottery&lt;T&gt;</c> implementation.
    /// </summary>
    [Serializable]
    public class Lottery<T> {
        private readonly List<(float Start, T Item)> _items = new();
        private readonly float _total;

        public Lottery(IEnumerable<(float Weight, T Item)> items) {
            float total = 0f;
            foreach (var (weight, item) in items) {
                _items.Add((total, item));
                total += weight;
            }
            _total = total;
        }

        /// <summary>Select an item using the provided random generator.</summary>
        public T Choose(Random rng) {
            if (_items.Count == 0) throw new InvalidOperationException("Lottery is empty");
            float x = (float)rng.NextDouble() * _total;
            int index = _items.BinarySearch((x, default!), Comparer);
            if (index < 0) index = ~index - 1;
            index = Math.Clamp(index, 0, _items.Count - 1);
            return _items[index].Item;
        }

        /// <summary>Select an item using a deterministic seed.</summary>
        public T ChooseSeeded(uint seed)
        {
            var rng = new Random(unchecked((int)seed));
            return Choose(rng);
        }

        /// <summary>Select an item using <see cref="Random"/>.</summary>
        public T Choose() => Choose(new Random());

        public IEnumerable<(float Weight, T Item)> Items =>
            _items.Zip(_items.Skip(1).Append((_total, default!)), (a,b) => (b.Start - a.Start, a.Item));

        public float Total => _total;

        private static readonly IComparer<(float Start, T Item)> Comparer = Comparer<(float Start, T Item)>.Create((a, b) => a.Start.CompareTo(b.Start));
    }
}
