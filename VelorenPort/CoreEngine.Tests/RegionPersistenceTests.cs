using System.IO;
using System.Linq;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class RegionPersistenceTests
{
    [Fact]
    public void SaveAndLoadHistory_PreservesEvents()
    {
        var region = new Region();
        region.Add(new Uid(1), null);
        region.Remove(new Uid(1), new int2(2,3));
        string path = Path.GetTempFileName();
        region.SaveHistory(path);

        var loaded = new Region();
        loaded.LoadHistory(path);
        File.Delete(path);

        Assert.Equal(2, loaded.History.Count);
        var first = loaded.History.First() as RegionEvent.Entered;
        Assert.NotNull(first);
        Assert.Equal((ulong)1, first!.Entity.Value);
        var second = loaded.History.Last() as RegionEvent.Left;
        Assert.NotNull(second);
        Assert.Equal((ulong)1, second!.Entity.Value);
        Assert.Equal(new int2(2,3), second.To!.Value);
    }
}
