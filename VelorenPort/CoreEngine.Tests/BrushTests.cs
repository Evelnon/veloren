using VelorenPort.CoreEngine.Terrain;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace CoreEngine.Tests;

public class BrushTests
{
    [Fact]
    public void FillBox_WritesExpectedCells()
    {
        var vol = new Vol<int>(new int3(3,3,1));
        Brush.FillBox(vol, int3.zero, new int3(2,2,1), 7);
        Assert.Equal(7, vol.Get(new int3(1,1,0)));
        Assert.Equal(0, vol.Get(new int3(2,2,0)));
    }

    [Fact]
    public void FillSphere_WritesWithinRadius()
    {
        var vol = new Vol<int>(new int3(5,5,5));
        Brush.FillSphere(vol, new int3(2,2,2), 1, 9);
        Assert.Equal(9, vol.Get(new int3(2,2,2)));
        Assert.Equal(0, vol.Get(new int3(0,0,0)));
    }
}
