using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Weather;

/// <summary>
/// Very small weather simulation used by tests and the server dispatcher.
/// It updates a <see cref="WeatherGrid"/> with random changes each tick.
/// </summary>
public class WeatherSim
{
    private readonly WeatherGrid _grid;
    private readonly Grid<float> _humidity;
    private readonly Random _rng;
    private readonly List<Zone> _zones = new();

    private struct Zone
    {
        public Weather Weather;
        public float2 Pos;
        public float Radius;
        public float Time;
    }

    public WeatherSim(int2 size, uint seed)
    {
        _grid = new WeatherGrid(size);
        _humidity = Grid<float>.PopulateFrom(size, _ => 0.5f);
        _rng = new Random((int)seed);
        foreach (var (pos, _) in _grid.Iterate())
            _grid.Set(pos, RandomWeather());
    }

    public WeatherGrid Grid => _grid;

    private Weather RandomWeather()
    {
        return new Weather(
            (float)_rng.NextDouble(),
            (float)_rng.NextDouble(),
            new float2((float)_rng.NextDouble() * 2f - 1f,
                        (float)_rng.NextDouble() * 2f - 1f));
    }

    /// <summary>Advance the simulation by one step.</summary>
    public void Tick(TimeOfDay time)
    {
        for (int i = _zones.Count - 1; i >= 0; i--)
        {
            var z = _zones[i];
            z.Time -= time.Value;
            if (z.Time <= 0f)
                _zones.RemoveAt(i);
            else
                _zones[i] = z;
        }

        foreach (var (pos, cell) in _grid.Iterate())
        {
            var w = ComputeWeather(pos, time);
            foreach (var z in _zones)
            {
                float2 p = (float2)pos + 0.5f;
                if (math.distance(p, z.Pos) <= z.Radius)
                {
                    w = z.Weather;
                    break;
                }
            }
            _grid.Set(pos, w);
        }
    }

    private Weather ComputeWeather(int2 cellPos, TimeOfDay time)
    {
        float2 wpos = (float2)cellPos * WeatherGrid.CellSize;
        float h = _humidity.Get(cellPos);
        float n = (float)_rng.NextDouble() * 2f - 1f;
        float pressure = math.clamp(0.55f + n * 0.2f - h * 0.6f, 0f, 1f);
        float cloud = math.pow(1f - pressure, 2f) * 4f;
        float rain = math.pow(math.max(1f - pressure - 0.25f, 0f) * h * 2.5f, 0.75f);
        float2 wind = new float2(
            (float)_rng.NextDouble() * 2f - 1f,
            (float)_rng.NextDouble() * 2f - 1f) * 200f * (1f - pressure);
        return new Weather(cloud, rain, wind);
    }

    public void AddZone(Weather weather, float2 pos, float radius, float time)
    {
        _zones.Add(new Zone { Weather = weather, Pos = pos, Radius = radius, Time = time });
    }
}
