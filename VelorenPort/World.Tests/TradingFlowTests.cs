using VelorenPort.World;
using VelorenPort.World.Site;
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
}
