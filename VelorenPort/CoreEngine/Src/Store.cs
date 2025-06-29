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
        private readonly Dictionary<Uid, float> _reputations = new();
        private readonly Random _rng;

        /// <summary>Maximum random fluctuation applied to prices.</summary>
        public float RandomFluctuationRange { get; set; } = 0.05f;

        public MerchantStore(int? seed = null)
        {
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public float GetReputation(Uid player) => _reputations.TryGetValue(player, out var r) ? r : 0f;

        public float Reputation => GetReputation(default);

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            IncludeFields = true,
            WriteIndented = false
        };

        public void AdjustReputation(Uid player, float delta)
            => _reputations[player] = Math.Clamp(GetReputation(player) + delta, -1f, 1f);

        public void AdjustReputation(float delta) => AdjustReputation(default, delta);

        public void AddItem(ItemDefinitionIdOwned item, uint amount, float basePrice)
            => Catalog.Add(item, amount, basePrice);

        public bool TryGetPrice(ItemDefinitionIdOwned item, uint amount, Uid player, out float price)
        {
            if (Catalog.TryGet(item, out var entry) && entry.Amount >= amount)
            {
                float supplyDemand = 1f + entry.Demand * 0.02f - entry.Amount * 0.01f;
                supplyDemand = Math.Clamp(supplyDemand, 0.5f, 2f);
                float fluctuation = RandomFluctuationRange == 0f ? 0f : (float)(_rng.NextDouble() * 2 * RandomFluctuationRange - RandomFluctuationRange);
                float basePrice = entry.BasePrice * supplyDemand * (1f + fluctuation);
                price = TradeUtils.ApplyReputation(basePrice, GetReputation(player)) * amount;
                return true;
            }
            price = 0f;
            return false;
        }

        public bool TryGetPrice(ItemDefinitionIdOwned item, uint amount, out float price)
            => TryGetPrice(item, amount, default, out price);

        public bool Buy(ItemDefinitionIdOwned item, uint amount, Uid player)
        {
            if (!Catalog.TryGet(item, out var entry) || entry.Amount < amount)
                return false;
            Catalog.Items[item] = new StoreCatalog.StoreEntry(entry.Amount - amount, entry.BasePrice, entry.Demand + amount);
            return true;
        }

        public bool Buy(ItemDefinitionIdOwned item, uint amount) => Buy(item, amount, default);

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
