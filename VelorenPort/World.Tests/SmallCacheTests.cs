using System.Collections.Generic;
using VelorenPort.World.Util;
using VelorenPort.NativeMath;
using Xunit;

namespace World.Tests;

public class SmallCacheTests
{
    private static IEnumerable<int> Int2Parts(int2 v)
    {
        yield return v.x;
        yield return v.y;
    }

    [Fact]
    public void Get_CachesValue()
    {
        var cache = new SmallCache<int2,int>(Int2Parts);
        int calls = 0;
        int v1 = cache.Get(new int2(1,2), k => { calls++; return k.x + k.y; });
        Assert.Equal(3, v1);
        Assert.Equal(1, calls);
        int v2 = cache.Get(new int2(1,2), k => { calls++; return k.x + k.y; });
        Assert.Equal(3, v2);
        Assert.Equal(1, calls);
    }
}
