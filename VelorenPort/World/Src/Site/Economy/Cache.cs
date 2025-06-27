using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VelorenPort.World.Site.Economy {
    /// <summary>
    /// Contains cached values used by the site economy. Rough port of
    /// <c>cache.rs</c>.
    /// </summary>
    [Serializable]
    public class EconomyCache {
        public GoodMap<float> TransportEffort { get; } = GoodMap<float>.FromDefault(1f);
        public GoodMap<float> DecayRate { get; } = GoodMap<float>.FromDefault(0f);
        public List<GoodIndex> DirectUseGoods { get; } = new();

        private static EconomyCache? _cache;
        public static EconomyCache Instance => _cache ??= LoadCache();

        private static EconomyCache LoadCache() {
            var cache = new EconomyCache();
            string path = Path.Combine("assets", "common", "economy", "trading_goods.ron");
            if (!File.Exists(path)) return cache;
            string text = File.ReadAllText(path);
            text = Regex.Replace(text, @"//.*$", string.Empty, RegexOptions.Multiline);
            var entryRegex = new Regex(@"(?m)^(\w+(?:\([A-Za-z]+\))?)\s*:\s*\(([^)]*)\)");
            foreach (Match m in entryRegex.Matches(text)) {
                string key = m.Groups[1].Value.Trim();
                string body = m.Groups[2].Value;
                float decay = 0f;
                float effort = 1f;
                bool storable = true;
                var fieldRegex = new Regex(@"(\w+)\s*:\s*([\d.]+|true|false)");
                foreach (Match fm in fieldRegex.Matches(body)) {
                    string fname = fm.Groups[1].Value;
                    string fval = fm.Groups[2].Value;
                    switch (fname) {
                        case "decay_rate": decay = float.Parse(fval); break;
                        case "transport_effort": effort = float.Parse(fval); break;
                        case "storable": storable = bool.Parse(fval); break;
                    }
                }
                if (TryParseGoodKey(key, out var good)) {
                    if (GoodIndex.TryFromGood(good, out var gi)) {
                        cache.DecayRate[gi] = decay;
                        cache.TransportEffort[gi] = effort;
                        if (!storable) cache.DirectUseGoods.Add(gi);
                    }
                }
            }
            return cache;
        }

        private static bool TryParseGoodKey(string key, out Good good) {
            if (key.StartsWith("Territory(")) {
                var b = key.Substring(10, key.Length - 11);
                if (Enum.TryParse(b, out BiomeKind biome)) { good = new Good.Territory(biome); return true; }
            } else if (key.StartsWith("Terrain(")) {
                var b = key.Substring(8, key.Length - 9);
                if (Enum.TryParse(b, out BiomeKind biome)) { good = new Good.Terrain(biome); return true; }
            } else {
                switch (key) {
                    case "Flour": good = new Good.Flour(); return true;
                    case "Meat": good = new Good.Meat(); return true;
                    case "Transportation": good = new Good.Transportation(); return true;
                    case "Food": good = new Good.Food(); return true;
                    case "Wood": good = new Good.Wood(); return true;
                    case "Stone": good = new Good.Stone(); return true;
                    case "Tools": good = new Good.Tools(); return true;
                    case "Armor": good = new Good.Armor(); return true;
                    case "Ingredients": good = new Good.Ingredients(); return true;
                    case "Potions": good = new Good.Potions(); return true;
                    case "Coin": good = new Good.Coin(); return true;
                    case "RoadSecurity": good = new Good.RoadSecurity(); return true;
                    case "Recipe": good = new Good.Recipe(); return true;
                    default: break;
                }
            }
            good = Good.Default; return false;
        }
    }
}
