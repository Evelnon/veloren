using System.Linq;
using VelorenPort.World.Util;
using VelorenPort.World;
using VelorenPort.NativeMath;
using Xunit;

namespace World.Tests;

public class StructureGenCacheTests
{
    [Fact]
    public void Get_CachesResults()
    {
        var gen = new StructureGen2d(1, 4, 0);
        var cache = new StructureGenCache<int>(gen);
        int calls = 0;
        var first = cache.Get(int2.zero, (p, s) => { calls++; return p.x + p.y; }).ToArray();
        Assert.Equal(9, calls);
        calls = 0;
        var second = cache.Get(int2.zero, (p, s) => { calls++; return p.x + p.y; }).ToArray();
        Assert.Equal(0, calls);
        Assert.Equal(first, second);
    }
}
