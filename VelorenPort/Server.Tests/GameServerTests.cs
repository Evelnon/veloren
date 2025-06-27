using System;
using VelorenPort.Server;
using VelorenPort.Network;

namespace Server.Tests;

public class GameServerTests
{
    [Fact]
    public void ConstructServer_InitializesWorldIndex()
    {
        var server = new GameServer(Pid.NewPid(), TimeSpan.FromMilliseconds(1), 42);
        Assert.Equal<uint>(42, server.WorldIndex.Seed);
    }

    [Fact]
    public void PidNewPid_ReturnsUniqueIds()
    {
        var first = Pid.NewPid();
        var second = Pid.NewPid();
        Assert.NotEqual(first, second);
    }
}
