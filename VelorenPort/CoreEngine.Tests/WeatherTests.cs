using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace CoreEngine.Tests;

public class WeatherTests
{
    [Fact]
    public void GetKind_ClassifiesCorrectly()
    {
        var w = new Weather(0.5f, 0.5f, float2.zero);
        Assert.Equal(WeatherKind.Rain, w.GetKind());
        w = new Weather(0.6f, 0f, float2.zero);
        Assert.Equal(WeatherKind.Cloudy, w.GetKind());
        w = new Weather(0f, 0f, new float2(30f,0f));
        Assert.Equal(WeatherKind.Storm, w.GetKind());
    }

    [Fact]
    public void Compressed_RoundTrip()
    {
        var w = new Weather(0.2f, 0.7f, float2.zero);
        CompressedWeather c = w;
        Weather back = c;
        Assert.True(math.abs(w.Cloud - back.Cloud) < 1f/255f + 1e-5f);
        Assert.True(math.abs(w.Rain - back.Rain) < 1f/255f + 1e-5f);
    }

    [Fact]
    public void Interpolation_Bilinear()
    {
        var grid = new WeatherGrid(new int2(2,2));
        grid.Set(new int2(0,0), new Weather(0f,0f,float2.zero));
        grid.Set(new int2(1,0), new Weather(1f,0f,float2.zero));
        grid.Set(new int2(0,1), new Weather(0f,1f,float2.zero));
        grid.Set(new int2(1,1), new Weather(1f,1f,float2.zero));
        var p = new float2(WeatherGrid.CellSize, WeatherGrid.CellSize);
        var w = grid.GetInterpolated(p);
        Assert.True(math.abs(w.Cloud - 0.5f) < 1e-5f);
        Assert.True(math.abs(w.Rain - 0.5f) < 1e-5f);
    }
}
