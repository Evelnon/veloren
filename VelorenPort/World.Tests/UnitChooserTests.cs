using VelorenPort.World;
using Unity.Mathematics;
using Xunit;

namespace World.Tests;

public class UnitChooserTests
{
    [Fact]
    public void Get_ReturnsDeterministicChoice()
    {
        var chooser = new UnitChooser(12345);
        var first = chooser.Get(0);
        Assert.Equal((new int2(1,0), new int2(0,1)), first);
        var second = chooser.Get(1);
        Assert.NotEqual(first, second);
        Assert.Contains(second, new[]
        {
            (new int2(1,0), new int2(0,-1)),
            (new int2(-1,0), new int2(0,1)),
            (new int2(-1,0), new int2(0,-1)),
            (new int2(0,1), new int2(1,0)),
            (new int2(0,1), new int2(-1,0)),
            (new int2(0,-1), new int2(1,0)),
            (new int2(0,-1), new int2(-1,0))
        });
    }
}
