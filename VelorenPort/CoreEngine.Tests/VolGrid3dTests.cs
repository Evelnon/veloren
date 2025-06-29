using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class VolGrid3dTests
{
    [Fact]
    public void SetAndGet_ReturnsStoredValue()
    {
        var grid = new VolGrid3d<int>(new int3(4,4,4), -1);
        grid.Set(new int3(5,3,2), 99);
        Assert.Equal(99, grid.Get(new int3(5,3,2)));
    }

    [Fact]
    public void MissingChunk_ReturnsDefault()
    {
        var grid = new VolGrid3d<int>(new int3(4,4,4), -1);
        Assert.Equal(-1, grid.Get(new int3(8,8,8)));
    }
}
