using System;
using VelorenPort.Server.Weather;
using VelorenPort.World;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace Server.Tests;

public class WeatherJobIntegrationTests
{
    [Fact]
    public void Tick_UpdatesWorldSimViaCallback()
    {
        var job = new WeatherJob();
        var sim = new WorldSim(0, new int2(1, 1));
        job.WeatherChanged += sim.ApplyGlobalWeather;
        var current = new Weather(0f, 0f, float2.zero);
        var target = new Weather(1f, 1f, new float2(1f, 0f));
        job.StartTransition(target, TimeSpan.FromSeconds(1), current, DateTime.UtcNow - TimeSpan.FromSeconds(2));
        bool changed = job.Tick(ref current);
        Assert.True(changed);
        Assert.Equal(target, current);
        var cell = sim.Weather.Grid.Get(int2.zero);
        Assert.Equal(target.Cloud, cell.Cloud, 3);
        Assert.Equal(target.Rain, cell.Rain, 3);
    }

    [Fact]
    public void Tick_UpdatesVisualEffects()
    {
        var job = new WeatherJob();
        WeatherEffects? updated = null;
        job.EffectsChanged += e => updated = e;
        var current = new Weather(0f, 0f, float2.zero);
        var target = new Weather(0.5f, 0.75f, new float2(1f, 2f));
        job.StartTransition(target, TimeSpan.FromSeconds(1), current, DateTime.UtcNow - TimeSpan.FromSeconds(2));

        job.Tick(ref current);

        Assert.NotNull(updated);
        Assert.Equal(target.Wind, updated!.Wind);
        Assert.Equal(target.Rain, updated.PrecipitationStrength, 3);
        Assert.Equal(new float3(target.Cloud, target.Cloud, target.Cloud), updated.CloudLayers);
    }
}
