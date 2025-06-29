using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace CoreEngine.Tests;

public class TimeResourcesTests
{
    [Fact]
    public void Time_AddDays_UsesCoefficient()
    {
        var time = new Time(0);
        var result = time.AddDays(1, 1.0);
        Assert.Equal(86400, result.Seconds);
    }

    [Fact]
    public void SunDirAndMoonDir_HaveOppositeZAtMidnight()
    {
        var tod = new TimeOfDay(0);
        float3 sun = tod.SunDir();
        float3 moon = tod.MoonDir();
        Assert.InRange(sun.z, 0.999f, 1.001f);
        Assert.InRange(moon.z, -1.001f, -0.999f);
    }
}
