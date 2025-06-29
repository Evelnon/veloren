using System.Collections.Generic;
using VelorenPort.Server.Weather;
using VelorenPort.CoreEngine;
using VelorenPort.Server.Ecs;
using VelorenPort.Network;
using VelorenPort.NativeMath;

namespace Server.Tests;

public class WeatherZoneIntegrationTests
{
    [Fact]
    public void Zone_AppliesWeatherThenExpires()
    {
        var index = new WorldIndex(1);
        var job = new WeatherJob();
        var sim = new WeatherSim(new int2(1,1), 1);
        var clients = new List<Client>();
        var sys = new WeatherTickSystem(index, job, sim, clients);
        var events = new EventManager();

        var w = new Weather(1f, 1f, float2.zero);
        job.QueueZone(w, new float2(0.5f,0.5f), 1f, 10f);

        sys.Update(5f, events);
        var cell = sim.Grid.Get(int2.zero);
        Assert.Equal(w.Cloud, cell.Cloud, 3);

        job.ClearZones();
        sys.Update(5f, events);
        cell = sim.Grid.Get(int2.zero);
        Assert.NotEqual(w.Cloud, cell.Cloud, 3);
    }
}
