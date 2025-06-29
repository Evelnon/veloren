using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using VelorenPort.CoreEngine;
using VelorenPort.World;
using CBiomeKind = VelorenPort.CoreEngine.BiomeKind;

namespace VelorenPort.World.Site.Economy;

/// <summary>
/// Aggregated resource data for a distance bucket.
/// </summary>
[Serializable]
public class AreaResources
{
    public GoodMap<float> ResourceSum { get; } = GoodMap<float>.FromDefault(0f);
    public GoodMap<float> ResourceChunks { get; } = GoodMap<float>.FromDefault(0f);
    public uint Chunks { get; set; } = 0;
}

/// <summary>
/// Natural resource tracking around a site.
/// </summary>
[Serializable]
public class NaturalResources
{
    public List<AreaResources> PerArea { get; } = new();
    public GoodMap<float> ChunksPerResource { get; } = GoodMap<float>.FromDefault(0f);
    public GoodMap<float> AverageYieldPerChunk { get; } = GoodMap<float>.FromDefault(0f);

    public void AddChunk(SimChunk chunk)
    {
        var area = new AreaResources();
        area.Chunks += 1;
        PerArea.Add(area);
    }
}

/// <summary>
/// Map keyed by <see cref="Labor"/> values.
/// </summary>
[Serializable]
public class LaborMap<T> where T : struct
{
    private readonly Dictionary<Labor, T> _data = new();

    public T this[Labor labor]
    {
        get => _data.TryGetValue(labor, out var v) ? v : default;
        set => _data[labor] = value;
    }

    public static LaborMap<T> FromDefault(T value)
    {
        var map = new LaborMap<T>();
        foreach (Labor l in Enum.GetValues(typeof(Labor)))
            map._data[l] = value;
        return map;
    }

    public IEnumerable<(Labor, T)> Iterate() => _data;
}

/// <summary>
/// Subset of professions used by the economy.
/// </summary>
[Serializable]
public enum Labor
{
    Farmer,
    Hunter,
    Blacksmith,
    Alchemist,
    Merchant,
    Guard,
    Everyone
}

/// <summary>
/// Profession definition parsed from <c>professions.ron</c>.
/// </summary>
[Serializable]
public record Profession(string Name, List<(GoodIndex, float)> Orders, (GoodIndex, float) Products)
{
    public static List<Profession> LoadDefaults()
    {
        var list = new List<Profession>();
        string path = Path.Combine("assets", "common", "professions.ron");
        if (!File.Exists(path)) return list;
        string text = File.ReadAllText(path);
        text = Regex.Replace(text, @"//.*$", string.Empty, RegexOptions.Multiline);
        var entryRegex = new Regex(@"\(\s*name:\s*\"(?<name>[^\"]+)\"[^\)]*orders:\s*\[(?<orders>[^\]]*)\]\s*,\s*products:\s*\[(?<products>[^\]]*)\]", RegexOptions.Singleline);
        foreach (Match m in entryRegex.Matches(text))
        {
            string name = m.Groups["name"].Value.Trim();
            var orders = ParseGoods(m.Groups["orders"].Value);
            var productsList = ParseGoods(m.Groups["products"].Value);
            (GoodIndex, float) prod = productsList.Count > 0 ? productsList[0] : (default, 0f);
            list.Add(new Profession(name, orders, prod));
        }
        return list;
    }

    private static List<(GoodIndex, float)> ParseGoods(string body)
    {
        var result = new List<(GoodIndex, float)>();
        var pairRegex = new Regex(@"\(([^,]+),\s*([\d.]+)\)");
        foreach (Match m in pairRegex.Matches(body))
        {
            string key = m.Groups[1].Value.Trim();
            float amt = float.Parse(m.Groups[2].Value);
            if (TryParseGoodKey(key, out var good) && GoodIndex.TryFromGood(good, out var gi))
                result.Add((gi, amt));
        }
        return result;
    }

    private static bool TryParseGoodKey(string key, out Good good)
    {
        if (key.StartsWith("Territory("))
        {
            var b = key.Substring(10, key.Length - 11);
            if (Enum.TryParse(b, out CBiomeKind biome)) { good = new Good.Territory(biome); return true; }
        }
        else if (key.StartsWith("Terrain("))
        {
            var b = key.Substring(8, key.Length - 9);
            if (Enum.TryParse(b, out CBiomeKind biome)) { good = new Good.Terrain(biome); return true; }
        }
        else
        {
            switch (key)
            {
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
            }
        }
        good = Good.Default; return false;
    }
}
