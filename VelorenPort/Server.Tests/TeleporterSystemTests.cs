using System;
using System.Reflection;
using VelorenPort.NativeMath;
using VelorenPort.Server;
using VelorenPort.Server.Sys;
using VelorenPort.Network;

namespace Server.Tests;

public class TeleporterSystemTests
{
    [Fact]
    public void Update_TeleportsClientWhenNearTeleporter()
    {
        var participant = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(1), Guid.NewGuid(), null, null, null })!;
        var client = (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { participant })!;
        client.SetPosition(new float3(0, 0, 0));

        var tp = new Teleporter { Position = new float3(0, 0, 0), Target = new float3(5, 5, 5) };
        TeleporterSystem.Update(new[] { client }, new[] { tp });

        Assert.Equal(new float3(5, 5, 5), client.Position.Value);
    }
}
