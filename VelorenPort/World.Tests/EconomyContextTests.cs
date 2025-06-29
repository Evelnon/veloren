using System.IO;
using VelorenPort.World.Site;
using VelorenPort.World.Site.Economy;
using VelorenPort.CoreEngine;

namespace World.Tests;

public class EconomyContextTests
{
    [Fact]
    public void Trade_RecordsMovement()
    {
        var index = new WorldIndex(0);
        var a = new Site { Position = VelorenPort.NativeMath.int2.zero };
        var b = new Site { Position = VelorenPort.NativeMath.int2.zero };
        var aId = index.Sites.Insert(a);
        var bId = index.Sites.Insert(b);
        Assert.True(GoodIndex.TryFromGood(new Good.Wood(), out var gi));
        a.Economy.Stocks[gi] += 5f;

        var ctx = new EconomyContext();
        ctx.Trade(index, aId, bId, new Good.Wood(), 2f);

        Assert.Single(ctx.History);
        Assert.Equal(3f, a.Economy.Stocks[gi]);
        Assert.Equal(2f, b.Economy.Stocks[gi]);
        Assert.True(ctx.SiteMetrics[aId].Exported > 0f);
        Assert.True(ctx.SiteMetrics[bId].Imported > 0f);
    }

    [Fact]
    public void Context_SerializesAndLoads()
    {
        var index = new WorldIndex(0);
        var a = new Site { Position = VelorenPort.NativeMath.int2.zero };
        var b = new Site { Position = VelorenPort.NativeMath.int2.zero };
        var aId = index.Sites.Insert(a);
        var bId = index.Sites.Insert(b);
        Assert.True(GoodIndex.TryFromGood(new Good.Stone(), out var gi2));
        a.Economy.Stocks[gi2] += 1f;
        var ctx = new EconomyContext();
        ctx.Trade(index, aId, bId, new Good.Stone(), 1f);

        var path = Path.GetTempFileName();
        ctx.Save(path);
        var loaded = EconomyContext.Load(path);
        File.Delete(path);

        Assert.Equal(ctx.History.Count, loaded.History.Count);
        Assert.Equal(ctx.SiteMetrics.Count, loaded.SiteMetrics.Count);
    }
}
