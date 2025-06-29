using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.Volumes;
using VelorenPort.NativeMath;

namespace CoreEngine.Tests;

public class ScaledVolTests
{
    [Fact]
    public void Get_ReturnsInnerScaledValue()
    {
        var inner = new Vol<int>(new int3(2,2,2));
        int count = 0;
        foreach (var pos in new DefaultPosEnumerator(int3.zero, inner.Size))
            inner.Set(pos, count++);
        var scaled = new ScaledVol<int>(inner, 2);
        Assert.Equal(inner.Get(new int3(0,0,0)), scaled.Get(new int3(0,0,0)));
        Assert.Equal(inner.Get(new int3(1,1,1)), scaled.Get(new int3(2,2,2)));
    }
}
