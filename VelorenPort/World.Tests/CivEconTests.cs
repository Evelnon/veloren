using System;
using System.Collections.Generic;
using VelorenPort.World.Civ;
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
}
