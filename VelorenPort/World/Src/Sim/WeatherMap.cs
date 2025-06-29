using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Sim;

/// <summary>
/// Simplified weather grid used by <see cref="WorldSim"/>. This roughly mirrors
/// the weather simulation storage from the Rust project and now includes
/// very small storm and lightning mechanics.
/// </summary>
[Serializable]
public class WeatherMap
{
    private readonly WeatherGrid _grid;
    private readonly Grid<float> _climate;
    private readonly List<Storm> _storms = new();
    private readonly List<LightningEvent> _lightning = new();

    /// <summary>Active storm with intensity and lifetime.</summary>
    [Serializable]
    public struct Storm
    {
        public int2 Center;
        public float Radius;
        public float Intensity;
        public float TimeLeft;
        public float2 Velocity;
    }

    /// <summary>Single lightning strike occurring during a tick.</summary>
    [Serializable]
    public struct LightningEvent
    {
        public float2 Pos;
    }

    private WeatherMap(WeatherGrid grid, Grid<float> climate)
    {
        _grid = grid;
        _climate = climate;
    }

    public WeatherGrid Grid => _grid;
    public Grid<float> Climate => _climate;
    public IReadOnlyList<Storm> ActiveStorms => _storms;
    public IReadOnlyList<LightningEvent> LightningEvents => _lightning;

    public void AddStorm(Storm storm) => _storms.Add(storm);

    /// <summary>Create a new weather map covering <paramref name="worldSize"/> chunks.</summary>
    public static WeatherMap Generate(int2 worldSize, uint seed)
    {
        var cells = new int2(
            math.max(1, (int)math.ceil(worldSize.x / (float)WeatherGrid.ChunksPerCell)),
            math.max(1, (int)math.ceil(worldSize.y / (float)WeatherGrid.ChunksPerCell)));
        var grid = new WeatherGrid(cells);
        var climate = Grid<float>.PopulateFrom(cells, p =>
            math.clamp((float)new Random((int)(seed + p.x * 31 + p.y * 17)).NextDouble(), 0f, 1f));
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
        return new WeatherMap(grid, climate);
    }

    /// <summary>Simple procedural update used during <see cref="WorldSim.Tick"/>.</summary>
    public void Tick(Random rng)
    {
        _lightning.Clear();

        float month = DateTime.UtcNow.Month;
        float seasonFactor = math.sin(month / 12f * math.PI * 2f) * 0.5f + 0.5f;

        foreach (var (pos, cell) in _grid.Iterate())
        {
            float regionFactor = math.clamp(pos.y / (float)math.max(1, _grid.Size.y - 1), 0f, 1f);
            float climate = _climate.Get(pos);
            float target = math.lerp(climate, seasonFactor, 0.5f);
            float cloud = math.clamp(cell.Cloud + ((float)rng.NextDouble() - 0.5f) * 0.02f + (target - cell.Cloud) * 0.05f + regionFactor * 0.01f, 0f, 1f);
            float rain = math.clamp(cell.Rain + ((float)rng.NextDouble() - 0.5f) * 0.02f + (target - cell.Rain) * 0.05f + regionFactor * 0.01f, 0f, 1f);
            float2 wind = cell.Wind + new float2(((float)rng.NextDouble() - 0.5f) * 0.5f,
                                                 ((float)rng.NextDouble() - 0.5f) * 0.5f);
            _grid.Set(pos, new Weather(cloud, rain, wind));
        }

        for (int i = _storms.Count - 1; i >= 0; i--)
        {
            var storm = _storms[i];
            storm.TimeLeft -= 1f;
            storm.Intensity *= 0.98f;
            storm.Center += (int2)math.round(storm.Velocity);
            storm.Velocity *= 0.95f;
            storm.Radius = math.clamp(storm.Radius + storm.Intensity * 0.1f, 0.5f, 10f);

            if (storm.TimeLeft <= 0f || storm.Intensity < 0.01f)
            {
                _storms.RemoveAt(i);
                continue;
            }

            foreach (var (pos, cell) in _grid.Iterate())
            {
                float dist = math.distance((float2)pos, (float2)storm.Center);
                if (dist > storm.Radius) continue;
                float factor = storm.Intensity * (1f - dist / storm.Radius);
                var w = new Weather(
                    math.clamp(cell.Cloud + factor * 0.5f, 0f, 1f),
                    math.clamp(cell.Rain + factor * 0.5f, 0f, 1f),
                    cell.Wind + new float2(((float)rng.NextDouble() * 2f - 1f) * factor,
                                           ((float)rng.NextDouble() * 2f - 1f) * factor));
                _grid.Set(pos, w);

                if (factor > 0.6f && rng.NextDouble() < 0.1)
                {
                    float2 wpos = (float2)pos * WeatherGrid.CellSize;
                    _lightning.Add(new LightningEvent { Pos = wpos });
                }
            }

            _storms[i] = storm;
        }
    }

    /// <summary>Interpolate weather at <paramref name="worldPos"/>.</summary>
    public Weather GetWeather(float2 worldPos) => _grid.GetInterpolated(worldPos);

    /// <summary>Set every cell to the provided <paramref name="weather"/>.</summary>
    public void ApplyGlobalWeather(Weather weather)
    {
        foreach (var (pos, _) in _grid.Iterate())
            _grid.Set(pos, weather);
    }

    private class WeatherDto
    {
        public int2 Size { get; set; }
        public List<CompressedWeather> Cells { get; set; } = new();
        public List<float> Climate { get; set; } = new();
        public List<Storm> Storms { get; set; } = new();
        public List<LightningEvent> Lightning { get; set; } = new();
    }

    /// <summary>Persist this weather map to <paramref name="path"/>.</summary>
    public void Save(string path)
    {
        var dto = new WeatherDto { Size = _grid.Size };
        var shared = SharedWeatherGrid.FromWeatherGrid(_grid);
        foreach (var (_, cell) in shared.Iterate())
            dto.Cells.Add(cell);
        foreach (var (_, c) in _climate.Iterate())
            dto.Climate.Add(c);
        dto.Storms.AddRange(_storms);
        dto.Lightning.AddRange(_lightning);
        File.WriteAllText(path, JsonSerializer.Serialize(dto));
    }

    /// <summary>Load a weather map from <paramref name="path"/> or return an empty map.</summary>
    public static WeatherMap Load(string path)
    {
        if (!File.Exists(path))
            return new WeatherMap(new WeatherGrid(new int2(0, 0)), new Grid<float>(new int2(0,0), 0f));
        var dto = JsonSerializer.Deserialize<WeatherDto>(File.ReadAllText(path)) ?? new WeatherDto();
        var grid = new WeatherGrid(dto.Size);
        var climate = new Grid<float>(dto.Size, 0f);
        int i = 0;
        foreach (var (pos, _) in grid.Iterate())
        {
            if (i < dto.Cells.Count)
                grid.Set(pos, (Weather)dto.Cells[i]);
            if (i < dto.Climate.Count)
                climate.Set(pos, dto.Climate[i]);
            i++;
        }
        var map = new WeatherMap(grid, climate);
        if (dto.Storms != null) map._storms.AddRange(dto.Storms);
        if (dto.Lightning != null) map._lightning.AddRange(dto.Lightning);
        return map;
    }
}
