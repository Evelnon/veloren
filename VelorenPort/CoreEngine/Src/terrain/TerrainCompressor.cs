using System.Collections.Generic;

namespace VelorenPort.CoreEngine.Terrain;

/// <summary>
/// Naïve run-length encoding compressor for voxel data. This is merely a helper
/// used by tests and does not aim to be efficient.
/// </summary>
public static class TerrainCompressor
{
    public static List<(T Value, int Count)> Compress<T>(IEnumerable<T> data)
    {
        var result = new List<(T,int)>();
        using var e = data.GetEnumerator();
        if (!e.MoveNext()) return result;
        var cur = e.Current;
        int count = 1;
        while (e.MoveNext())
        {
            if (EqualityComparer<T>.Default.Equals(cur, e.Current))
            {
                count++;
            }
            else
            {
                result.Add((cur, count));
                cur = e.Current;
                count = 1;
            }
        }
        result.Add((cur, count));
        return result;
    }

    public static IEnumerable<T> Decompress<T>(IEnumerable<(T Value, int Count)> data)
    {
        foreach (var (val, count) in data)
            for (int i = 0; i < count; i++)
                yield return val;
    }
}
