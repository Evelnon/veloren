using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class VolGrid2dTests
{
    [Fact]
    public void SetAndGet_ReturnsStoredValue()
    {
        var grid = new VolGrid2d<int>(new int2(2,2), new int3(4,4,1), -1);
        grid.Set(new int3(5,3,0), 42);
        Assert.Equal(42, grid.Get(new int3(5,3,0)));
    }

    [Fact]
    public void MissingChunk_ReturnsDefault()
    {
        var grid = new VolGrid2d<int>(new int2(1,1), new int3(4,4,1), -1);
        Assert.Equal(-1, grid.Get(new int3(2,2,0)));
    }
}
