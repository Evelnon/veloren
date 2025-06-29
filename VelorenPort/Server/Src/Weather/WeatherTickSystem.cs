using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;
using VelorenPort.Network;
using VelorenPort.Server.Ecs;

namespace VelorenPort.Server.Weather;

/// <summary>
/// Dispatcher system that advances the simple weather simulation and sends
/// updates to connected clients.
/// </summary>
public class WeatherTickSystem : IGameSystem
{
    private const float WeatherDt = 5f;
    private readonly WorldIndex _index;
    private readonly WeatherJob _job;
    private readonly WeatherSim _sim;
    private readonly IList<Client> _clients;
    private float _accumulator;

    public WeatherTickSystem(WorldIndex index, WeatherJob job, WeatherSim sim, IList<Client> clients)
    {
        _index = index;
        _job = job;
        _sim = sim;
        _clients = clients;
    }

    public void Update(float dt, Events.EventManager events)
    {
        _accumulator += dt;
        bool changed = _job.Tick(ref _index.CurrentWeather);

        if (_accumulator >= WeatherDt)
        {
            _accumulator -= WeatherDt;
            _sim.Tick();
            var w = _sim.Grid.Get(int2.zero);
            _job.StartTransition(w, TimeSpan.FromSeconds(2), _index.CurrentWeather);
            changed = true;
        }

        if (changed)
            Broadcast(_index.CurrentWeather);
    }

    private void Broadcast(Weather weather)
    {
        var msg = PreparedMsg.Create(
            0,
            new ServerGeneral.WeatherUpdate(weather),
            new StreamParams(Promises.Ordered));
        foreach (var c in _clients)
            c.SendPreparedAsync(msg).GetAwaiter().GetResult();
    }
}
