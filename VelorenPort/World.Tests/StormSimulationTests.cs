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
            TimeLeft = 2f
        };
        map.AddStorm(storm);
        var rng = new Random(1);
        map.Tick(rng);
        Assert.Single(map.ActiveStorms);
        Assert.NotEmpty(map.LightningEvents);
        map.Tick(rng);
        Assert.Empty(map.ActiveStorms);
    }
}
