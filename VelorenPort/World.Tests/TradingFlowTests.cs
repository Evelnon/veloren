using VelorenPort.World;
using VelorenPort.World.Site;
using VelorenPort.World.Site.Economy;
using VelorenPort.NativeMath;
using Xunit;

namespace World.Tests;

public class TradingFlowTests
{
    [Fact]
    public void WorldTick_UpdatesEconomyAndCaravans()
    {
        var (world, index) = World.World.Empty();
        var siteA = new Site { Position = int2.zero };
        var siteB = new Site { Position = new int2(5, 0) };
        var idA = index.Sites.Insert(siteA);
        var idB = index.Sites.Insert(siteB);
        siteA.Economy.Produce(new Good.Wood(), 4f);
        index.Caravans.Add(new Caravan(new[] { idA, idB }));

        for (int i = 0; i < 3; i++)
            world.Tick(1f);

        Assert.True(siteB.Economy.GetStock(new Good.Wood()) > 0f);
        Assert.NotEmpty(index.EconomyContext.Events);
        Assert.True(index.EconomyContext.MarketPrices.ContainsKey(idA));
    }

    [Fact]
    public void WorldTick_RecordsTradePhases()
    {
        var (world, index) = World.World.Empty();
        var siteA = new Site { Position = int2.zero };
        var siteB = new Site { Position = new int2(5, 0) };
        var idA = index.Sites.Insert(siteA);
        var idB = index.Sites.Insert(siteB);
        siteA.Economy.Produce(new Good.Food(), 2f);
        index.Caravans.Add(new Caravan(new[] { idA, idB }));

        world.Tick(1f);
        world.Tick(1f);

        Assert.Contains(index.EconomyContext.Events, e => e.Phase == EconomyContext.TradePhase.Plan);
        Assert.Contains(index.EconomyContext.Events, e => e.Phase == EconomyContext.TradePhase.Execute);
        Assert.Contains(index.EconomyContext.Events, e => e.Phase == EconomyContext.TradePhase.Collect);
        Assert.NotEmpty(index.EconomyContext.StageHistory);
    }

    [Fact]
    public void TradingRoute_RecordsTradeEvents()
    {
        var (world, index) = World.World.Empty();
        var siteA = new Site { Position = int2.zero };
        var siteB = new Site { Position = new int2(4, 0) };
        var idA = index.Sites.Insert(siteA);
        var idB = index.Sites.Insert(siteB);
        var route = new CaravanRoute(new[] { idA, idB });
        if (GoodIndex.TryFromGood(new Good.Wood(), out var gi))
            route.Goods[gi] = 1f;
        index.CaravanRoutes.Add(route);
        siteA.Economy.Produce(new Good.Wood(), 2f);

        world.Tick(1f);

        Assert.Contains(index.EconomyContext.Events, e => e.Phase == EconomyContext.TradePhase.Plan);
        Assert.Contains(index.EconomyContext.Events, e => e.Phase == EconomyContext.TradePhase.Execute);
        Assert.Contains(index.EconomyContext.Events, e => e.Phase == EconomyContext.TradePhase.Collect);
    }
}
