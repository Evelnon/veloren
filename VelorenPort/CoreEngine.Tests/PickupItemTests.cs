using VelorenPort.CoreEngine.comp;
using Xunit;

namespace CoreEngine.Tests;

public class PickupItemTests
{
    [Fact]
    public void Take_RemovesAmountAndReturnsTaken()
    {
        var item = new PickupItem("wood", 5, true);
        var taken = item.Take(3);
        Assert.Equal((uint)3, taken);
        Assert.Equal((uint)2, item.Amount);
    }
}
