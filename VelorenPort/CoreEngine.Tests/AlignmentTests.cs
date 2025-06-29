using VelorenPort.CoreEngine.comp;
using VelorenPort.CoreEngine;
using Xunit;

namespace CoreEngine.Tests;

public class AlignmentTests
{
    [Fact]
    public void HostileTowardsWorks()
    {
        Alignment self = new Alignment.Enemy();
        Alignment other = new Alignment.Npc();
        Assert.True(self.HostileTowards(other));
        Assert.False(other.HostileTowards(self));
    }

    [Fact]
    public void FriendlyTowardsOwned()
    {
        var a = new Alignment.Owned(new Uid(1));
        var b = new Alignment.Owned(new Uid(1));
        Assert.True(a.FriendlyTowards(b));
        Assert.True(b.FriendlyTowards(a));
    }
}
