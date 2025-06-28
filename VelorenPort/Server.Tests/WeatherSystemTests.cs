using System;
using System.Linq;
using VelorenPort.CoreEngine;
using VelorenPort.Server;
using VelorenPort.Server.Sys;
using VelorenPort.Server.Weather;
using VelorenPort.Network;
using Unity.Mathematics;

namespace Server.Tests;

public class WeatherSystemTests
{
    [Fact]
    public void Update_ChangesWeatherAfterTimeout()
    {
        var index = new WorldIndex(1);
        var job = new WeatherJob { NextUpdate = DateTime.UtcNow }; // immediate
        WeatherSystem.Update(index, job, Enumerable.Empty<Client>());
        Assert.True(job.NextUpdate > DateTime.UtcNow);
        Assert.NotEqual(new Weather(0f, 0f, float2.zero), index.CurrentWeather);
    }

    [Fact]
    public void Update_SkipsWhenNotDue()
    {
        var index = new WorldIndex(1);
        var job = new WeatherJob { NextUpdate = DateTime.UtcNow + TimeSpan.FromHours(1) };
        var prev = index.CurrentWeather;
        WeatherSystem.Update(index, job, Enumerable.Empty<Client>());
        Assert.Equal(prev, index.CurrentWeather);
    }
}
