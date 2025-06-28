using VelorenPort.CoreEngine;
using Xunit;

namespace World.Tests;

public class RandomPermTests
{
    [Fact]
    public void Get_ReturnsExpectedValue()
    {
        var rp = new RandomPerm(12345);
        uint v = rp.Get(42);
        Assert.Equal(2745238656u, v);
    }

    [Fact]
    public void NextU32_UpdatesSeed()
    {
        var rp = new RandomPerm(12345);
        uint next = rp.NextU32();
        Assert.Equal(0x9315E29Au, next);
    }
}
