using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class DayPeriodUtilTests
{
    [Theory]
    [InlineData(0, DayPeriod.Night)]
    [InlineData(7 * 3600, DayPeriod.Morning)]
    [InlineData(12 * 3600, DayPeriod.Noon)]
    [InlineData(18 * 3600, DayPeriod.Evening)]
    [InlineData(22 * 3600, DayPeriod.Night)]
    [InlineData(-1 * 3600, DayPeriod.Night)]
    public void FromTimeOfDay_ReturnsExpectedPeriod(double seconds, DayPeriod expected)
    {
        Assert.Equal(expected, DayPeriodUtil.FromTimeOfDay(seconds));
    }

    [Fact]
    public void IsDarkAndLight_WorkAsOpposites()
    {
        Assert.True(DayPeriod.Night.IsDark());
        Assert.False(DayPeriod.Night.IsLight());
        Assert.False(DayPeriod.Noon.IsDark());
        Assert.True(DayPeriod.Noon.IsLight());
    }
}
