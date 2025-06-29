using System;
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
    private readonly Random _rng;

    public WeatherSim(int2 size, uint seed)
    {
        _grid = new WeatherGrid(size);
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
    public void Tick()
    {
        foreach (var (pos, cell) in _grid.Iterate())
        {
            float cloud = math.clamp(cell.Cloud + ((float)_rng.NextDouble() - 0.5f) * 0.1f, 0f, 1f);
            float rain = math.clamp(cell.Rain + ((float)_rng.NextDouble() - 0.5f) * 0.1f, 0f, 1f);
            float2 wind = cell.Wind + new float2(((float)_rng.NextDouble() - 0.5f) * 0.1f,
                                                 ((float)_rng.NextDouble() - 0.5f) * 0.1f);
            _grid.Set(pos, new Weather(cloud, rain, wind));
        }
    }
}
