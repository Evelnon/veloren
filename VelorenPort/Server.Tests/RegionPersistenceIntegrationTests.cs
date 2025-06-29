using System;
using System.IO;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace Server.Tests;

public class RegionPersistenceIntegrationTests
{
    [Fact]
    public void RegionMap_PersistsAcrossRestarts()
    {
        string dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var map = new RegionMap(dir);
        var uid = new Uid(99);
        map.Set(uid, new int2(1, 2));
        map.Flush();

        var loaded = new RegionMap(dir);
        var pos = loaded.GetRegion(uid);
        Assert.True(pos.HasValue);
        Assert.Equal(new int2(1, 2), pos!.Value);
        Directory.Delete(dir, true);
    }
}
