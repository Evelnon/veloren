using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class CmdArgumentSpecTests
{
    [Fact]
    public void UsageString_RequiredPlayer()
    {
        var arg = new ArgumentSpec.PlayerName(Requirement.Required);
        Assert.Equal("<player>", arg.UsageString());
    }
}
