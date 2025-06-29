using VelorenPort.Server.Weather;
using VelorenPort.CoreEngine;
using VelorenPort.Server.Ecs;
using VelorenPort.Server.Events;
using VelorenPort.Network;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace Server.Tests;

public class WeatherTickSystemTests
{
    [Fact]
    public void Update_AfterInterval_ChangesWeather()
    {
        var index = new WorldIndex(1);
        var job = new WeatherJob();
        var sim = new WeatherSim(new int2(1,1), 1);
        var clients = new List<Client>();
        var sys = new WeatherTickSystem(index, job, sim, clients);
        var events = new EventManager();

        var w0 = index.CurrentWeather;
        sys.Update(1f, events); // accumulate 1 second
        Assert.Equal(w0, index.CurrentWeather);
        sys.Update(5f, events); // total > WEATHER_DT
        Assert.NotEqual(w0, index.CurrentWeather);
    }
}
