using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Sim;

/// <summary>
/// Simplified weather grid used by <see cref="WorldSim"/>. This roughly mirrors
/// the weather simulation storage from the Rust project but does not yet
/// simulate storms or lightning.
/// </summary>
[Serializable]
public class WeatherMap
{
    private readonly WeatherGrid _grid;

    private WeatherMap(WeatherGrid grid)
    {
        _grid = grid;
    }

    public WeatherGrid Grid => _grid;

    /// <summary>Create a new weather map covering <paramref name="worldSize"/> chunks.</summary>
    public static WeatherMap Generate(int2 worldSize, uint seed)
    {
        var cells = new int2(
            math.max(1, (int)math.ceil(worldSize.x / (float)WeatherGrid.ChunksPerCell)),
            math.max(1, (int)math.ceil(worldSize.y / (float)WeatherGrid.ChunksPerCell)));
        var grid = new WeatherGrid(cells);
        var rng = new Random((int)seed);
        foreach (var (pos, _) in grid.Iterate())
        {
            var w = new Weather(
                (float)rng.NextDouble(),
                (float)rng.NextDouble() * 0.3f,
                new float2((float)rng.NextDouble() * 5f - 2.5f,
                           (float)rng.NextDouble() * 5f - 2.5f));
            grid.Set(pos, w);
        }
        return new WeatherMap(grid);
    }

    /// <summary>Simple procedural update used during <see cref="WorldSim.Tick"/>.</summary>
    public void Tick(Random rng)
    {
        foreach (var (pos, cell) in _grid.Iterate())
        {
            float cloud = math.clamp(cell.Cloud + ((float)rng.NextDouble() - 0.5f) * 0.02f, 0f, 1f);
            float rain = math.clamp(cell.Rain + ((float)rng.NextDouble() - 0.5f) * 0.02f, 0f, 1f);
            float2 wind = cell.Wind + new float2(((float)rng.NextDouble() - 0.5f) * 0.5f,
                                                ((float)rng.NextDouble() - 0.5f) * 0.5f);
            _grid.Set(pos, new Weather(cloud, rain, wind));
        }
    }

    /// <summary>Interpolate weather at <paramref name="worldPos"/>.</summary>
    public Weather GetWeather(float2 worldPos) => _grid.GetInterpolated(worldPos);

    private class WeatherDto
    {
        public int2 Size { get; set; }
        public List<CompressedWeather> Cells { get; set; } = new();
    }

    /// <summary>Persist this weather map to <paramref name="path"/>.</summary>
    public void Save(string path)
    {
        var dto = new WeatherDto { Size = _grid.Size };
        var shared = SharedWeatherGrid.FromWeatherGrid(_grid);
        foreach (var (_, cell) in shared.Iterate())
            dto.Cells.Add(cell);
        File.WriteAllText(path, JsonSerializer.Serialize(dto));
    }

    /// <summary>Load a weather map from <paramref name="path"/> or return an empty map.</summary>
    public static WeatherMap Load(string path)
    {
        if (!File.Exists(path))
            return new WeatherMap(new WeatherGrid(new int2(0, 0)));
        var dto = JsonSerializer.Deserialize<WeatherDto>(File.ReadAllText(path)) ?? new WeatherDto();
        var grid = new WeatherGrid(dto.Size);
        int i = 0;
        foreach (var (pos, _) in grid.Iterate())
        {
            if (i < dto.Cells.Count)
                grid.Set(pos, (Weather)dto.Cells[i]);
            i++;
        }
        return new WeatherMap(grid);
    }
}
