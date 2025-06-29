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
    [Serializable]
    public struct ClimateCell
    {
        public float BaseTemp;
        public float BaseRain;
        public float Storminess;
    }

    private readonly Grid<ClimateCell> _climate;
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

    private WeatherMap(WeatherGrid grid, Grid<ClimateCell> climate)
    {
        _grid = grid;
        _climate = climate;
    }

    public WeatherGrid Grid => _grid;
    public Grid<ClimateCell> Climate => _climate;
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
        var rng = new Random((int)seed);
        var climate = Grid<ClimateCell>.PopulateFrom(cells, p =>
        {
            var local = new Random((int)(seed + p.x * 31 + p.y * 17));
            return new ClimateCell
            {
                BaseTemp = (float)local.NextDouble() * 2f - 1f,
                BaseRain = (float)local.NextDouble(),
                Storminess = (float)local.NextDouble() * 0.2f
            };
        });
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
    public void Tick(Random rng, int month = -1)
    {
        _lightning.Clear();

        if (month < 0) month = DateTime.UtcNow.Month;
        float seasonFactor = math.sin(month / 12f * math.PI * 2f) * 0.5f + 0.5f;

        foreach (var (pos, cell) in _grid.Iterate())
        {
            float regionFactor = math.clamp(pos.y / (float)math.max(1, _grid.Size.y - 1), 0f, 1f);
            var climate = _climate.Get(pos);
            float temp = climate.BaseTemp + (seasonFactor - 0.5f) * 2f - regionFactor;
            float rainBase = math.clamp(climate.BaseRain + seasonFactor * 0.2f, 0f, 1f);
            float target = math.lerp(rainBase, cell.Rain, 0.5f);
            float cloud = math.clamp(cell.Cloud + ((float)rng.NextDouble() - 0.5f) * 0.02f + (target - cell.Cloud) * 0.05f, 0f, 1f);
            float rain = math.clamp(cell.Rain + ((float)rng.NextDouble() - 0.5f) * 0.02f + (target - cell.Rain) * 0.05f, 0f, 1f);
            float2 wind = cell.Wind + new float2(((float)rng.NextDouble() - 0.5f) * 0.5f,
                                                 ((float)rng.NextDouble() - 0.5f) * 0.5f);
            _grid.Set(pos, new Weather(cloud, rain, wind));

            if (rng.NextDouble() < climate.Storminess * rain)
            {
                var storm = new Storm
                {
                    Center = pos,
                    Radius = 1f,
                    Intensity = rain,
                    TimeLeft = 5f,
                    Velocity = new float2(((float)rng.NextDouble() - 0.5f) * 2f,
                                         ((float)rng.NextDouble() - 0.5f) * 2f)
                };
                _storms.Add(storm);
            }
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
        public List<ClimateCell> Climate { get; set; } = new();
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
            return new WeatherMap(new WeatherGrid(new int2(0, 0)), new Grid<ClimateCell>(new int2(0,0), new ClimateCell()));
        var dto = JsonSerializer.Deserialize<WeatherDto>(File.ReadAllText(path)) ?? new WeatherDto();
        var grid = new WeatherGrid(dto.Size);
        var climate = new Grid<ClimateCell>(dto.Size, new ClimateCell());
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
