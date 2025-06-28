using VelorenPort.CoreEngine;
using Unity.Mathematics;

namespace CoreEngine.Tests;

public class ColorUtilTests
{
    [Fact]
    public void LinearSrgbRoundTrip()
    {
        var color = new Rgb<float>(0.5f, 0.1f, 0.8f);
        var srgb = ColorUtil.LinearToSrgb(color);
        var back = ColorUtil.SrgbToLinear(srgb);
        var diff = new float3(color.R - back.R, color.G - back.G, color.B - back.B);
        Assert.True(math.length(diff) < 1e-3f);
    }

    [Fact]
    public void HsvRgbRoundTrip()
    {
        var hsv = new float3(240f, 0.5f, 0.8f);
        var rgb = ColorUtil.HsvToRgb(hsv);
        var back = ColorUtil.RgbToHsv(rgb);
        var diff = hsv - back;
        Assert.True(math.length(diff) < 1e-3f);
    }
}
