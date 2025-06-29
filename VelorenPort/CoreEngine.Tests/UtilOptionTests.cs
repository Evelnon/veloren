using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class UtilOptionTests
{
    [Fact]
    public void EitherWith_ValueTypes()
    {
        int? a = 2;
        int? b = 3;
        int? res = OptionUtil.EitherWith(a, b, (x, y) => x + y);
        Assert.Equal(5, res);
    }
}
