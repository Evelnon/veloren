using VelorenPort.World.Site;
using VelorenPort.World;
using VelorenPort.NativeMath;
using Xunit;

namespace World.Tests;

public class CaravanTests
{
    [Fact]
    public void Caravan_MovesGoodsBetweenSites()
    {
        var index = new WorldIndex(0);
        var siteA = new Site { Position = int2.zero };
        var siteB = new Site { Position = new int2(10,0) };
        var idA = index.Sites.Insert(siteA);
        var idB = index.Sites.Insert(siteB);
        Assert.True(GoodIndex.TryFromGood(new Good.Food(), out var gi));
        siteA.Economy.Stocks[gi] += 3f;

        var caravan = new Caravan(new[] { idA, idB });
        for (int i = 0; i < 2; i++)
        {
            EconomySim.SimulateCaravans(index, new[] { caravan }, 1f);
            EconomySim.SimulateEconomy(index, 1f);
        }

        Assert.True(siteB.Economy.Stocks[gi] > 0f);
    }
}
