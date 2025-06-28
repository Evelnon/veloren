using VelorenPort.World.Site.Util;
using VelorenPort.CoreEngine;
using Unity.Mathematics;

namespace World.Tests;

public class GradientTests
{
    [Fact]
    public void Sample_ReturnsInterpolatedColor()
    {
        var g = new Gradient(float3.zero, 10f, Shape.Point, (new Rgb8(0,0,0), new Rgb8(100,0,0)));
        var col = g.Sample(new float3(5f, 0f, 0f));
        Assert.Equal((byte)50, col.R);
    }

    [Fact]
    public void Repeat_WrapsDistance()
    {
        var g = new Gradient(float3.zero, 10f, Shape.Point, (new Rgb8(0,0,0), new Rgb8(100,0,0))).WithRepeat(WrapMode.Repeat);
        var col = g.Sample(new float3(25f, 0f, 0f));
        Assert.Equal((byte)50, col.R);
    }
}

