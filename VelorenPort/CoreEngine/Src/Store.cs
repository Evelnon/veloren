using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using VelorenPort.CoreEngine.Store;

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

        public void Remove(Id id)
        {
            var idx = (int)id.Value;
            if (idx < 0 || idx >= _items.Count) return;
            _items[idx] = default!;
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

    /// <summary>
    /// Inventory used by merchant NPCs. Supports dynamic catalogs and
    /// reputation-based pricing.
    /// </summary>
    [Serializable]
    public class MerchantStore
    {
        public StoreCatalog Catalog { get; } = new();
        public float Reputation { get; private set; }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            IncludeFields = true,
            WriteIndented = false
        };

        public void AdjustReputation(float delta)
            => Reputation = Math.Clamp(Reputation + delta, -1f, 1f);

        public void AddItem(ItemDefinitionIdOwned item, uint amount, float price)
            => Catalog.Add(item, amount, price);

        public bool TryGetPrice(ItemDefinitionIdOwned item, uint amount, out float price)
        {
            if (Catalog.TryGet(item, out var entry) && entry.Amount >= amount)
            {
                price = TradeUtils.ApplyReputation(entry.Price, Reputation) * amount;
                return true;
            }
            price = 0f;
            return false;
        }

        public bool Buy(ItemDefinitionIdOwned item, uint amount)
        {
            if (!Catalog.TryGet(item, out var entry) || entry.Amount < amount)
                return false;
            Catalog.Items[item] = new StoreCatalog.StoreEntry(entry.Amount - amount, entry.Price);
            return true;
        }

        public void Save(string path)
        {
            var map = new Dictionary<string, StoreCatalog.StoreEntry>();
            foreach (var kv in Catalog.Items)
            {
                string keyStr = kv.Key switch
                {
                    ItemDefinitionIdOwned.Simple s => s.Id,
                    _ => kv.Key.ToString()
                };
                map[keyStr] = kv.Value;
            }
            File.WriteAllText(path, JsonSerializer.Serialize(map, JsonOpts));
        }

        public void Load(string path)
        {
            if (!File.Exists(path)) return;
            var loaded = JsonSerializer.Deserialize<Dictionary<string, StoreCatalog.StoreEntry>>(File.ReadAllText(path), JsonOpts);
            if (loaded != null)
            {
                Catalog.Items.Clear();
                foreach (var kv in loaded)
                    Catalog.Items[new ItemDefinitionIdOwned.Simple(kv.Key)] = kv.Value;
            }
        }
    }
}
