using System;
using System.Linq;
using VelorenPort.CoreEngine;
using VelorenPort.Server;
using VelorenPort.Server.Sys;
using VelorenPort.Server.Weather;
using VelorenPort.Network;
using VelorenPort.NativeMath;

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

    [Fact]
    public void QueueZone_ForcesWeatherUntilCleared()
    {
        var index = new WorldIndex(1);
        var job = new WeatherJob { NextUpdate = DateTime.UtcNow + TimeSpan.FromHours(1) };
        var zoneWeather = new Weather(1f, 0f, float2.zero);
        job.QueueZone(zoneWeather, TimeSpan.FromMinutes(5));

        WeatherSystem.Update(index, job, Enumerable.Empty<Client>());
        Assert.Equal(zoneWeather, index.CurrentWeather);

        WeatherSystem.Update(index, job, Enumerable.Empty<Client>());
        Assert.Equal(zoneWeather, index.CurrentWeather);

        job.ClearZones();
        job.NextUpdate = DateTime.UtcNow;
        WeatherSystem.Update(index, job, Enumerable.Empty<Client>());
        Assert.NotEqual(zoneWeather, index.CurrentWeather);
    }

    [Fact]
    public void StartTransition_SmoothlyLerpsWeather()
    {
        var index = new WorldIndex(1);
        var job = new WeatherJob();
        var start = index.CurrentWeather;
        var target = new Weather(1f, 1f, new float2(1, 1));
        var now = DateTime.UtcNow - TimeSpan.FromSeconds(1);
        job.StartTransition(target, TimeSpan.FromSeconds(2), start, now);

        // halfway through transition
        job.Tick(ref index.CurrentWeather);
        var mid = index.CurrentWeather;
        Assert.NotEqual(start, mid);
        Assert.NotEqual(target, mid);

        // end of transition
        job.StartTransition(target, TimeSpan.FromSeconds(1), mid, DateTime.UtcNow - TimeSpan.FromSeconds(2));
        job.Tick(ref index.CurrentWeather);
        Assert.Equal(target, index.CurrentWeather);
    }
}
