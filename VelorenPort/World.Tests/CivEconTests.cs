using System;
using System.Collections.Generic;
using System.Linq;
using VelorenPort.World.Civ;
using VelorenPort.World.Site;
using Xunit;

namespace World.Tests;

public class CivEconTests
{
    [Fact]
    public void BuyUnits_BuysCheapestAvailable()
    {
        var rng = new Random(1);
        var sell = new List<SellOrder>
        {
            new SellOrder { Quantity = 5f, Price = 2f },
            new SellOrder { Quantity = 2f, Price = 1f }
        };
        var (q, spent) = Econ.BuyUnits(rng, sell, 3f, 3f, 10f);
        Assert.Equal(3f, q, 3);
        Assert.True(spent >= 4f && spent <= 6f);
    }

    [Fact]
    public void Tick_RecordsStageOrder()
    {
        var (world, index) = World.World.Empty();
        var a = new Site.Site { Position = VelorenPort.NativeMath.int2.zero };
        var b = new Site.Site { Position = new VelorenPort.NativeMath.int2(1, 0) };
        var idA = index.Sites.Insert(a);
        var idB = index.Sites.Insert(b);
        index.Caravans.Add(new Caravan(new[] { idA, idB }));

        index.EconomyContext.Tick(index, 1f);

        var stages = index.EconomyContext.StageHistory.Select(s => s.Stage).ToArray();
        Assert.Equal(new[] { EconomyStage.Deliveries, EconomyStage.TickSites, EconomyStage.UpdateMarkets }, stages);
    }
}
