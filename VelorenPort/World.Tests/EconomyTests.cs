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
        Assert.True(EconomySim.TradeGoods(a, b, new Good.Wood(), 2f));
        Assert.Equal(2f, a.Economy.GetStock(new Good.Wood()));
        Assert.Equal(2f, b.Economy.GetStock(new Good.Wood()));
    }

    [Fact]
    public void Market_BuyAndSell_ModifiesCoinAndStock()
    {
        var site = new Site { Position = Unity.Mathematics.int2.zero };
        site.Economy.Coin = 10f;
        site.Economy.Market.SetPrice(new Good.Wood(), 2f);

        Assert.True(site.Economy.Market.Buy(site.Economy, new Good.Wood(), 3f));
        Assert.Equal(4f, site.Economy.Coin);
        Assert.Equal(3f, site.Economy.GetStock(new Good.Wood()));

        Assert.True(site.Economy.Market.Sell(site.Economy, new Good.Wood(), 1f));
        Assert.Equal(6f, site.Economy.Coin);
        Assert.Equal(2f, site.Economy.GetStock(new Good.Wood()));
    }

    [Fact]
    public void CaravanRoute_TransfersGoodsBetweenSites()
    {
        EconomySim.ClearCaravanRoutes();
        var a = new Site { Position = Unity.Mathematics.int2.zero };
        var b = new Site { Position = Unity.Mathematics.int2.one };
        a.Economy.Produce(new Good.Stone(), 5f);
        a.Economy.Market.SetPrice(new Good.Stone(), 1f);
        b.Economy.Market.SetPrice(new Good.Stone(), 1f);

        var route = new Economy.CaravanRoute(a, b, new Good.Stone(), 2f, 1f);
        EconomySim.AddCaravanRoute(route);
        EconomySim.SimulateEconomy(new WorldIndex(0), 1.1f);

        Assert.Equal(3f, a.Economy.GetStock(new Good.Stone()));
        Assert.Equal(2f, b.Economy.GetStock(new Good.Stone()));
    }

    [Fact]
    public void Market_DynamicPricing_ChangesWithStock()
    {
        var site = new Site { Position = Unity.Mathematics.int2.zero };
        site.Economy.Coin = 100f;
        site.Economy.Market.SetPrice(new Good.Wood(), 2f);

        float basePrice = site.Economy.Market.GetPrice(new Good.Wood());

        site.Economy.Produce(new Good.Wood(), 5f);
        Assert.True(site.Economy.Market.Sell(site.Economy, new Good.Wood(), 5f));
        float cheap = site.Economy.Market.GetPrice(new Good.Wood());
        Assert.True(cheap < basePrice);

        Assert.True(site.Economy.Market.Buy(site.Economy, new Good.Wood(), 5f));
        float expensive = site.Economy.Market.GetPrice(new Good.Wood());
        Assert.True(expensive > cheap);
    }
}
