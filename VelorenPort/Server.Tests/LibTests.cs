using System;
using VelorenPort.Server;
using VelorenPort.CoreEngine;

namespace Server.Tests;

public class LibTests
{
    [Fact]
    public void SpawnPoint_Default_HasExpectedZ()
    {
        var def = SpawnPoint.Default;
        Assert.Equal(256f, def.Position.z);
    }

    [Fact]
    public void BattleModeBuffer_PushPopRoundTrip()
    {
        var buf = new BattleModeBuffer();
        var id = new CharacterId(1);
        var time = new Time(10);
        buf.Push(id, (BattleMode.PvE, time));
        var popped = buf.Pop(id);
        Assert.NotNull(popped);
        Assert.Equal(BattleMode.PvE, popped?.Mode);
        Assert.Equal(time.Seconds, popped?.Time.Seconds);
    }
}
