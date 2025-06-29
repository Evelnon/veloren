using VelorenPort.World.Site;
using VelorenPort.CoreEngine;
using Xunit;

namespace World.Tests;

public class MarketTests
{
    [Fact]
    public void AddDemand_AccumulatesDemand()
    {
        var market = new Economy.Market();
        market.AddDemand(new Good.Wood(), 3f);
        market.AddDemand(new Good.Wood(), 2f);
        Assert.Equal(5f, market.GetDemand(new Good.Wood()));
    }

    [Fact]
    public void UpdatePrices_RespondsToDemand()
    {
        var site = new Site { Position = VelorenPort.NativeMath.int2.zero };
        Assert.True(GoodIndex.TryFromGood(new Good.Wood(), out var gi));
        site.Economy.Stocks[gi] += 1f;
        site.Market.AddDemand(new Good.Wood(), 5f);
        site.Market.UpdatePrices(site.Economy);

        Assert.True(site.Market.GetPrice(new Good.Wood()) > 1f);
    }
}
