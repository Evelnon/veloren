using VelorenPort.CoreEngine;
using Xunit;

namespace World.Tests;

public class MathUtilTests
{
    [Fact]
    public void CloseFast_BehavesLikeClamp()
    {
        float v1 = MathUtil.CloseFast(0f, 0f, 1f, 1);
        Assert.Equal(1f, v1, 3);
        float v2 = MathUtil.CloseFast(2f, 0f, 1f, 1);
        Assert.Equal(0f, v2, 3);
    }
}
