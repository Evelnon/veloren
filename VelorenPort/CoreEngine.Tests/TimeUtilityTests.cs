using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;
using Xunit;

namespace CoreEngine.Tests;

public class TimeUtilityTests
{
    [Fact]
    public void TimeOfDay_DayWrapsAround()
    {
        var tod = new TimeOfDay(90000); // 25 hours
        Assert.InRange(tod.Day, 3599, 3601);
    }

    [Fact]
    public void Secs_MultiplicationOperators()
    {
        var secs = new Secs(10);
        var doubleFirst = secs * 2;
        var doubleSecond = 2 * secs;
        Assert.Equal(20, doubleFirst.Value);
        Assert.Equal(20, doubleSecond.Value);
        secs.MultiplyAssign(0.5);
        Assert.Equal(5, secs.Value);
    }
}
