using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Sim;

/// <summary>
/// Simplified humidity map storing a single float value per chunk.
/// Mirrors the concept of a humidity field used during world simulation
/// but does not yet implement diffusion or erosion.
/// </summary>
[Serializable]
public class HumidityMap
{
    private readonly Grid<float> _map;

    private HumidityMap(Grid<float> map)
    {
        _map = map;
    }


    /// <summary>Create a new humidity map with the given size and value.</summary>
    public HumidityMap(int2 size, float initial = 0.5f)
    {
        _map = new Grid<float>(size, initial);
    }

    /// <summary>Create a new humidity map for a map of <paramref name="size"/> chunks.</summary>
    public static HumidityMap Generate(int2 size, float initial = 0.5f)
    {
        var grid = Grid<float>.PopulateFrom(size, _ => initial);
        return new HumidityMap(grid);
    }

    /// <summary>Create a new humidity map based on an existing world.</summary>
    public static HumidityMap Generate(World world, float initial = 0.5f)
        => Generate(world.Sim.GetSize(), initial);

    /// <summary>Get the humidity value at the given chunk coordinate.</summary>
    public float Get(int2 key) => _map.Get(key) ?? 0f;

    /// <summary>Set the humidity value at the given chunk coordinate.</summary>
    public void Set(int2 key, float value) => _map.Set(key, value);

    public int2 Size => _map.Size;

    public IEnumerable<(int2 Pos, float Value)> Iterate() => _map.Iterate();

    /// <summary>
    /// Diffuse humidity across neighbouring chunks using a simple averaging
    /// model. <paramref name="strength"/> controls how much of the neighbour
    /// average is blended into each cell (0..1).
    /// </summary>
    public void Diffuse(float strength = 0.25f)
    {
        var next = new Grid<float>(_map.Size, 0f);
        foreach (var (pos, value) in _map.Iterate())
        {
            float sum = value;
            int count = 1;
            foreach (var off in WorldUtil.NEIGHBORS)
            {
                int2 n = pos + off;
                if (_map.TryGet(n, out float v))
                {
                    sum += v;
                    count++;
                }
            }
            float avg = sum / count;
            next.Set(pos, math.lerp(value, avg, strength));
        }

        foreach (var (pos, val) in next.Iterate())
            _map.Set(pos, val);
    }

    /// <summary>
    /// Persist this humidity map as JSON at <paramref name="path"/>.
    /// </summary>
    public void Save(string path)
    {
        var dto = new HumidityDto
        {
            Size = _map.Size,
            Data = new List<float>()
        };
        foreach (var (_, v) in _map.Iterate())
            dto.Data.Add(v);
        File.WriteAllText(path, JsonSerializer.Serialize(dto));
    }

    /// <summary>
    /// Load a humidity map from <paramref name="path"/> or return a new map
    /// if the file is missing.
    /// </summary>
    public static HumidityMap Load(string path)
    {
        if (!File.Exists(path))
            return new HumidityMap(new Grid<float>(new int2(0, 0), 0f));
        var dto = JsonSerializer.Deserialize<HumidityDto>(File.ReadAllText(path)) ?? new HumidityDto();
        var grid = new Grid<float>(dto.Size, dto.Data);
        return new HumidityMap(grid);
    }

    private class HumidityDto
    {
        public int2 Size { get; set; }
        public List<float> Data { get; set; } = new();
    }
}
