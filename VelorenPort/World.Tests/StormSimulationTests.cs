using System;
using VelorenPort.World.Sim;
using VelorenPort.NativeMath;

namespace World.Tests;

public class StormSimulationTests
{
    [Fact]
    public void Storm_DissipatesAfterDuration()
    {
        var map = WeatherMap.Generate(new int2(4,4), 1);
        var storm = new WeatherMap.Storm
        {
            Center = new int2(1,1),
            Radius = 1f,
            Intensity = 1f,
            TimeLeft = 2f,
            Velocity = float2.zero
        };
        map.AddStorm(storm);
        var rng = new Random(1);
        map.Tick(rng);
        Assert.Single(map.ActiveStorms);
        Assert.NotEmpty(map.LightningEvents);
        map.Tick(rng);
        Assert.Empty(map.ActiveStorms);
    }

    [Fact]
    public void Storm_MovesAccordingToVelocity()
    {
        var map = WeatherMap.Generate(new int2(4,4), 1);
        var storm = new WeatherMap.Storm
        {
            Center = new int2(0,0),
            Radius = 1f,
            Intensity = 1f,
            TimeLeft = 3f,
            Velocity = new float2(1f,0f)
        };
        map.AddStorm(storm);
        var rng = new Random(1);
        map.Tick(rng);
        Assert.Equal(new int2(1,0), map.ActiveStorms[0].Center);
    }
}
