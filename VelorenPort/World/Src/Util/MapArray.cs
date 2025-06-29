using System;
using System.Collections.Generic;

namespace VelorenPort.World.Util;

/// <summary>
/// Simple array indexed by an enum type. Rough translation of Rust's
/// <c>map_array</c> utilities.
/// </summary>
public class MapArray<TEnum, TValue> where TEnum : Enum
{
    private readonly TValue[] _data;

    public MapArray(TValue defaultValue)
    {
        var values = Enum.GetValues<TEnum>();
        _data = new TValue[values.Length];
        for (int i = 0; i < _data.Length; i++)
            _data[i] = defaultValue;
    }

    public TValue this[TEnum key]
    {
        get => _data[Convert.ToInt32(key)];
        set => _data[Convert.ToInt32(key)] = value;
    }

    public IEnumerable<(TEnum Key, TValue Value)> Items()
    {
        foreach (TEnum e in Enum.GetValues<TEnum>())
            yield return (e, this[e]);
    }
}

/// <summary>Helper functions for mapping between enums and indices.</summary>
public static class EnumIndex
{
    public static int IndexFromEnum<TEnum>(TEnum value) where TEnum : Enum
        => Array.IndexOf(Enum.GetValues<TEnum>(), value);

    public static TEnum EnumFromIndex<TEnum>(int index) where TEnum : Enum
    {
        var arr = Enum.GetValues<TEnum>();
        return (TEnum)arr.GetValue(index)!;
    }
}
