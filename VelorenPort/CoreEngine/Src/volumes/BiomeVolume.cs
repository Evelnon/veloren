using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine.Terrain;

namespace VelorenPort.CoreEngine.Volumes;

/// <summary>
/// 3D volume storing <see cref="BiomeKind"/> values with helpers to
/// serialize to a compressed representation.
/// </summary>
[Serializable]
public class BiomeVolume
{
    private readonly Vol<BiomeKind> _vol;

    public BiomeVolume(int3 size, BiomeKind fill)
    {
        _vol = new Vol<BiomeKind>(size);
        _vol.Fill(fill);
    }

    public int3 Size => _vol.Size;

    public IEnumerable<(int3 Pos, BiomeKind Biome)> Cells() => _vol.Cells();

    public BiomeKind Get(int3 pos) => _vol.Get(pos);

    public void Set(int3 pos, BiomeKind value) => _vol.Set(pos, value);

    public SharedBiomeVolume ToShared()
    {
        var list = new List<CompressedBiome>();
        foreach (var (_, biome) in _vol.Cells())
            list.Add((CompressedBiome)biome);
        var compressed = TerrainCompressor.Compress(list);
        return new SharedBiomeVolume(_vol.Size, compressed);
    }

    public static BiomeVolume FromShared(SharedBiomeVolume shared)
    {
        var vol = new BiomeVolume(shared.Size, BiomeKind.Void);
        var data = TerrainCompressor.Decompress(shared.Data);
        int idx = 0;
        foreach (var pos in new DefaultPosEnumerator(int3.zero, vol.Size))
        {
            if (idx >= data.Count) break;
            vol.Set(pos, (BiomeKind)data[idx]);
            idx++;
        }
        return vol;
    }
}

/// <summary>
/// Serializable representation of a <see cref="BiomeVolume"/> using run-length
/// encoding via <see cref="TerrainCompressor"/>.
/// </summary>
[Serializable]
public class SharedBiomeVolume
{
    public int3 Size { get; set; }
    public List<(CompressedBiome Value, int Count)> Data { get; set; }

    public SharedBiomeVolume(int3 size, List<(CompressedBiome Value, int Count)> data)
    {
        Size = size;
        Data = data;
    }
}
