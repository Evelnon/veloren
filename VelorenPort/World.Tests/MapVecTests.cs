using VelorenPort.World.Util;
using Xunit;

namespace World.Tests;

public class MapVecTests
{
    [Fact]
    public void Get_ReturnsDefaultWhenMissing()
    {
        var mv = new MapVec<int, int>(0);
        Assert.Equal(0, mv.Get(5));
    }

    [Fact]
    public void Set_StoresValue()
    {
        var mv = new MapVec<string, int>(-1);
        mv.Set("hello", 42);
        Assert.Equal(42, mv["hello"]);
    }
}
