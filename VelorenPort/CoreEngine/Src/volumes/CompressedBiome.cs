using System;
using VelorenPort.CoreEngine;

namespace VelorenPort.CoreEngine.Volumes;

/// <summary>
/// Byte-sized representation of a <see cref="BiomeKind"/> for compact storage.
/// </summary>
[Serializable]
public struct CompressedBiome
{
    public byte Value;

    public static implicit operator CompressedBiome(BiomeKind biome)
    {
        return new CompressedBiome { Value = (byte)biome };
    }

    public static implicit operator BiomeKind(CompressedBiome cb)
    {
        return (BiomeKind)cb.Value;
    }
}
