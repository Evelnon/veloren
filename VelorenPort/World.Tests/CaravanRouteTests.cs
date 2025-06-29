using VelorenPort.World;
using VelorenPort.World.Site;
using VelorenPort.World.Site.Economy;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using Xunit;

namespace World.Tests;

public class CaravanRouteTests
{
    [Fact]
    public void GenerateLinear_ReturnsIdsInOrder()
    {
        var store = new Store<Site.Site>();
        var a = store.Insert(new Site.Site());
        var b = store.Insert(new Site.Site());
        var route = CaravanRoute.GenerateLinear(new[] { a, b });
        Assert.Equal(new[] { a, b }, route.Sites);
    }

    [Fact]
    public void Tick_TransfersGoodsBetweenSites()
    {
        var index = new WorldIndex(0);
        var siteA = new Site.Site { Position = int2.zero };
        var siteB = new Site.Site { Position = new int2(5,0) };
        var idA = index.Sites.Insert(siteA);
        var idB = index.Sites.Insert(siteB);
        siteA.Production.SetRate(new Good.Wood(), 2f);
        var route = new CaravanRoute(new[] { idA, idB });
        if (GoodIndex.TryFromGood(new Good.Wood(), out var gi))
            route.Goods[gi] = 1f;
        index.CaravanRoutes.Add(route);
        var sim = new WorldSim(0, new int2(8,8));
        for (int i = 0; i < 3; i++)
            sim.Tick(index, 1f);
        Assert.True(GoodIndex.TryFromGood(new Good.Wood(), out var gi1));
        Assert.True(siteB.Economy.Stocks[gi1] > 0f);
    }

    [Fact]
    public void Tick_MultiSiteRouteTransfersGoods()
    {
        var index = new WorldIndex(0);
        var siteA = new Site.Site { Position = int2.zero };
        var siteB = new Site.Site { Position = new int2(2,0) };
        var siteC = new Site.Site { Position = new int2(4,0) };
        var idA = index.Sites.Insert(siteA);
        var idB = index.Sites.Insert(siteB);
        var idC = index.Sites.Insert(siteC);
        Assert.True(GoodIndex.TryFromGood(new Good.Stone(), out var gi0));
        siteA.Economy.Stocks[gi0] += 3f;
        var route = new CaravanRoute(new[] { idA, idB, idC });
        if (GoodIndex.TryFromGood(new Good.Stone(), out var gi))
            route.Goods[gi] = 1f;
        index.CaravanRoutes.Add(route);
        var sim = new WorldSim(0, new int2(8,8));
        for (int i = 0; i < 4; i++)
            sim.Tick(index, 1f);
        Assert.True(siteC.Economy.Stocks[gi0] > 0f);
    }
}
