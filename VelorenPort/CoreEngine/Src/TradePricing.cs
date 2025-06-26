using System;
using System.Collections.Generic;
using System.Linq;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public class EqualitySet {
        private readonly Dictionary<ItemDefinitionIdOwned, ItemDefinitionIdOwned> _classes = new();

        public ItemDefinitionIdOwned Canonical(ItemDefinitionIdOwned item) {
            return _classes.TryGetValue(item, out var canonical) ? canonical : item;
        }

        public void AddSet(IEnumerable<ItemDefinitionIdOwned> items) {
            ItemDefinitionIdOwned? canonical = null;
            foreach (var item in items) {
                canonical ??= item;
                _classes[item] = canonical;
            }
        }
    }
    [Serializable]
    public class PriceEntry {
        public ItemDefinitionIdOwned Name { get; }
        public MaterialUse Price { get; private set; }
        public bool Sell { get; }
        public bool Stackable { get; }

        public PriceEntry(ItemDefinitionIdOwned name, MaterialUse price, bool sell, bool stackable) {
            Name = name;
            Price = price;
            Sell = sell;
            Stackable = stackable;
        }

        internal void MergeAlternative(PriceEntry other) {
            var freq1 = MaterialFrequency.FromMaterialUse(Price);
            var freq2 = MaterialFrequency.FromMaterialUse(other.Price);
            var combined = freq1 + freq2;
            Price = combined.ToMaterialUse();
        }
    }

    [Serializable]
    public class TradePricing {
        private static readonly TradePricing _instance = new();
        public static TradePricing Instance => _instance;

        private readonly Dictionary<ItemDefinitionIdOwned, PriceEntry> _items = new();
        private readonly EqualitySet _equality = new();

        public void Register(ItemDefinitionIdOwned item, IEnumerable<(float Amount, Good Material)> materials, bool sell = true, bool stackable = true) {
            var canonical = _equality.Canonical(item);
            var entry = new PriceEntry(canonical, new MaterialUse(materials), sell, stackable);
            if (_items.TryGetValue(canonical, out var existing)) {
                existing.MergeAlternative(entry);
            } else {
                _items[canonical] = entry;
            }
        }

        private List<(float Amount, Good Material)>? GetMaterialsImpl(ItemDefinitionIdOwned item) {
            var canonical = _equality.Canonical(item);
            return _items.TryGetValue(canonical, out var entry) ? entry.Price.Values : null;
        }

        public static List<(float Amount, Good Material)>? GetMaterials(ItemDefinitionIdOwned item) =>
            Instance.GetMaterialsImpl(item);

        public void AddEquivalentSet(IEnumerable<ItemDefinitionIdOwned> items) {
            _equality.AddSet(items);
        }

        private List<(ItemDefinitionIdOwned Item, uint Amount)> RandomItemsImpl(
            Dictionary<Good, float> stock,
            uint number,
            bool selling,
            bool alwaysCoin,
            uint limit) {
            static float Stock(Dictionary<Good, float> s, Good g) => s.TryGetValue(g, out var v) ? v : 0f;
            var candidates = _items.Values
                .Where(i => !i.Price.Values.Any(v => v.Amount >= Stock(stock, v.Material))
                            && (!selling || i.Sell)
                            && (!alwaysCoin || !(i.Name is ItemDefinitionIdOwned.Simple s && s.Id == "common.items.utility.coins")))
                .ToList();
            var result = new List<(ItemDefinitionIdOwned, uint)>();
            if (alwaysCoin && number > 0) {
                uint amount = (uint)Math.Floor(Stock(stock, new Good.Coin()));
                if (amount > 0) {
                    result.Add((new ItemDefinitionIdOwned.Simple("common.items.utility.coins"), amount));
                    number -= 1;
                }
            }

            var rng = new Random();
            for (int n = 0; n < number; n++) {
                candidates.RemoveAll(c => c.Price.Values.Any(v => v.Amount >= Stock(stock, v.Material)));
                if (candidates.Count == 0) break;
                int index = rng.Next(candidates.Count);
                var entry = candidates[index];
                uint amount = 1;
                if (entry.Stackable) {
                    float maxAmount = entry.Price.Values
                        .Select(v => Stock(stock, v.Material) / Math.Max(v.Amount, 0.001f))
                        .DefaultIfEmpty(float.PositiveInfinity)
                        .Min();
                    maxAmount = MathF.Min(maxAmount, limit);
                    amount = (uint)Math.Floor((float)rng.NextDouble() * (maxAmount - 1f)) + 1u;
                }
                foreach (var p in entry.Price.Values) {
                    if (stock.TryGetValue(p.Material, out var val)) {
                        stock[p.Material] = val - p.Amount * amount;
                    }
                }
                result.Add((entry.Name, amount));
                candidates.RemoveAt(index);
            }
            return result;
        }

        public static List<(ItemDefinitionIdOwned Item, uint Amount)> RandomItems(
            Dictionary<Good, float> stock,
            uint number,
            bool selling,
            bool alwaysCoin,
            uint limit) =>
            Instance.RandomItemsImpl(stock, number, selling, alwaysCoin, limit);

    }
}
