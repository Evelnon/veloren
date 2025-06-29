using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Sim;

/// <summary>
/// Simplified representation of natural resources per chunk.
/// Mirrors <c>rtsim/src/data/nature.rs</c> in a reduced form.
/// </summary>
[Serializable]
public class Nature
{
    private readonly Grid<Chunk> _chunks;

    private Nature(Grid<Chunk> chunks)
    {
        _chunks = chunks;
    }

    /// <summary>Create a new resource map based on the given world size.</summary>
    public static Nature Generate(World world)
        => Generate(world.Sim.GetSize());

    public static Nature Generate(int2 size)
    {
        var grid = Grid<Chunk>.PopulateFrom(size, _ => Chunk.Default());
        return new Nature(grid);
    }

    private NatureDto ToDto()
    {
        var dto = new NatureDto { Size = _chunks.Size, Chunks = new List<Chunk>() };
        foreach (var (_, c) in _chunks.Iterate())
            dto.Chunks.Add(c);
        return dto;
    }

    /// <summary>Persist this map to <paramref name="path"/>.</summary>
    public void Save(string path)
        => File.WriteAllText(path, JsonSerializer.Serialize(ToDto()));

    /// <summary>Load a nature map from <paramref name="path"/> or create a new one.</summary>
    public static Nature Load(string path)
    {
        if (!File.Exists(path))
            return new Nature(new Grid<Chunk>(new int2(0, 0), Chunk.Default()));
        var dto = JsonSerializer.Deserialize<NatureDto>(File.ReadAllText(path)) ?? new NatureDto();
        var grid = new Grid<Chunk>(dto.Size, dto.Chunks);
        return new Nature(grid);
    }

    /// <summary>Retrieve resource factors for the chunk at <paramref name="key"/>.</summary>
    public Dictionary<ChunkResource, float> GetChunkResources(int2 key)
    {
        var chunk = _chunks.Get(key);
        return chunk == null
            ? new Dictionary<ChunkResource, float>()
            : new Dictionary<ChunkResource, float>(chunk.Res);
    }

    /// <summary>Update resource factors for the chunk at <paramref name="key"/>.</summary>
    public void SetChunkResources(int2 key, Dictionary<ChunkResource, float> res)
    {
        var chunk = _chunks.Get(key);
        if (chunk != null)
            chunk.Res = new Dictionary<ChunkResource, float>(res);
    }
}

/// <summary>Resource proportions for a chunk.</summary>
[Serializable]
public class Chunk
{
    public Dictionary<ChunkResource, float> Res = new();

    public static Chunk Default()
    {
        var chunk = new Chunk();
        foreach (ChunkResource r in Enum.GetValues(typeof(ChunkResource)))
            chunk.Res[r] = 1f;
        return chunk;
    }
}

[Serializable]
internal class NatureDto
{
    public int2 Size { get; set; }
    public List<Chunk> Chunks { get; set; } = new();
}

