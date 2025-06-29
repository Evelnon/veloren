using System.IO;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;
using Xunit;

namespace World.Tests;

public class RegionMapPersistenceTests
{
    [Fact]
    public void RegionMap_SerializesAndLoads()
    {
        var map = new RegionMap();
        var uid = new Uid(42);
        map.Set(uid, new int2(2,3));
        var path = Path.GetTempFileName();
        map.Save(path);
        var loaded = RegionMap.Load(path);
        var pos = loaded.GetRegion(uid);
        Assert.True(pos.HasValue);
        Assert.Equal(new int2(2,3), pos!.Value);
        File.Delete(path);
    }
}
