using VelorenPort.World.Util;
using VelorenPort.NativeMath;
using Xunit;

namespace World.Tests;

public class WGridTests
{
    [Fact]
    public void GetLocal_ReturnsStoredValue()
    {
        var grid = new WGrid<int>(1, 2, 0);
        grid.SetLocal(int2.zero, 42);
        Assert.Equal(42, grid.GetLocal(int2.zero));
        Assert.Equal(42, grid.GetWorld(new int2(1, 1)));
    }
}
