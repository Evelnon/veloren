using VelorenPort.World;
using VelorenPort.NativeMath;

namespace World.Tests;

public class WorldUtilTests
{
    [Fact]
    public void WithinDistance_Works()
    {
        Assert.True(WorldUtil.WithinDistance(int2.zero, new int2(3,4), 5));
        Assert.False(WorldUtil.WithinDistance(int2.zero, new int2(5,5), 5));
    }
}
