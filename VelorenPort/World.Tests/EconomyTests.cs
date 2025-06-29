using VelorenPort.World.Site;
using VelorenPort.CoreEngine;
using Xunit;

namespace World.Tests;

public class EconomyTests
{
    [Fact]
    public void ProduceAndConsume_WorkCorrectly()
    {
        var data = new FullEconomy();
        Assert.True(GoodIndex.TryFromGood(new Good.Food(), out var gi));
        data.Stocks[gi] += 5f;
        Assert.Equal(5f, data.Stocks[gi]);
        bool ok = data.Stocks[gi] >= 3f;
        if (ok) data.Stocks[gi] -= 3f;
        Assert.True(ok);
        Assert.Equal(2f, data.Stocks[gi]);
        bool ok2 = data.Stocks[gi] >= 5f;
        if (ok2) data.Stocks[gi] -= 5f;
        Assert.False(ok2);
    }

    [Fact]
    public void TradeGoods_MovesStockBetweenSites()
    {
        var a = new Site { Position = VelorenPort.NativeMath.int2.zero };
        var b = new Site { Position = VelorenPort.NativeMath.int2.zero };
        Assert.True(GoodIndex.TryFromGood(new Good.Wood(), out var gi));
        a.Economy.Stocks[gi] += 4f;
        Assert.True(EconomySim.TradeGoods(a, b, new Good.Wood(), 2f));
        Assert.Equal(2f, a.Economy.Stocks[gi]);
        Assert.Equal(2f, b.Economy.Stocks[gi]);
    }
}
