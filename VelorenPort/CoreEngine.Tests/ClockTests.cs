using System;
using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class ClockTests
{
    [Fact]
    public void TickAdvancesTime()
    {
        var clock = new Clock(TimeSpan.FromMilliseconds(1));
        clock.Tick();
        Assert.True(clock.TotalTickTime > TimeSpan.Zero);
    }

    [Fact]
    public void GetStableDt_ReturnsRecentDt()
    {
        var clock = new Clock(TimeSpan.FromMilliseconds(1));
        for (int i = 0; i < 6; i++)
        {
            clock.Tick();
        }
        var stable = clock.GetStableDt();
        Assert.True(stable > TimeSpan.Zero);
    }

    [Fact]
    public void Result_Ok_HoldsValue()
    {
        var result = Result<int, string>.Ok(5);
        Assert.True(result.IsOk);
        Assert.Equal(5, result.Value);
    }
}
