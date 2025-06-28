using VelorenPort.World.Site;
using VelorenPort.CoreEngine;
using Xunit;

namespace World.Tests;

public class EconomyTests
{
    [Fact]
    public void ProduceAndConsume_WorkCorrectly()
    {
        var data = new EconomyData();
        data.Produce(new Good.Food(), 5f);
        Assert.Equal(5f, data.GetStock(new Good.Food()));
        Assert.True(data.Consume(new Good.Food(), 3f));
        Assert.Equal(2f, data.GetStock(new Good.Food()));
        Assert.False(data.Consume(new Good.Food(), 5f));
    }

    [Fact]
    public void TradeGoods_MovesStockBetweenSites()
    {
        var a = new Site { Position = Unity.Mathematics.int2.zero };
        var b = new Site { Position = Unity.Mathematics.int2.zero };
        a.Economy.Produce(new Good.Wood(), 4f);
        Assert.True(Economy.TradeGoods(a, b, new Good.Wood(), 2f));
        Assert.Equal(2f, a.Economy.GetStock(new Good.Wood()));
        Assert.Equal(2f, b.Economy.GetStock(new Good.Wood()));
    }
}
