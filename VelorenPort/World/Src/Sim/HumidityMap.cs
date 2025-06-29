using System;
using System.Collections.Generic;
using Unity.Mathematics;
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

    /// <summary>Create a new humidity map covering the world's dimensions.</summary>
    public static HumidityMap Generate(World world, float initial = 0.5f)
    {
        int2 size = world.Sim.GetSize();
        var grid = Grid<float>.PopulateFrom(size, _ => initial);
        return new HumidityMap(grid);
    }

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
    /// Run diffusion repeatedly to allow humidity to propagate further across
    /// the map. This mirrors the iterative approach of the original Rust code
    /// but without complex boundary conditions.
    /// </summary>
    public void RunDiffusion(int iterations, float strength = 0.25f)
    {
        for (int i = 0; i < iterations; i++)
            Diffuse(strength);
    }
}
