using VelorenPort.World.Sim;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace World.Tests;

public class PathTests
{
    [Fact]
    public void HeadSpace_ComputesExpected()
    {
        var path = Path.Default;
        Assert.Equal(8, path.HeadSpace(0f));
        Assert.Equal(1, path.HeadSpace(10f));
    }

    [Fact]
    public void SurfaceColor_AddsNoise()
    {
        var path = Path.Default;
        var col = new Rgb8(100, 0, 0);
        var wpos = new int3(3, 6, 9);
        var res = path.SurfaceColor(col, wpos);
        Assert.Equal((byte)77, res.R);
        Assert.Equal((byte)7, res.G);
        Assert.Equal((byte)7, res.B);
    }
}
