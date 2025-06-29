using System;
using System.Reflection;
using Unity.Entities;
using Unity.Mathematics;
using VelorenPort.Server;
using VelorenPort.Server.Sys;
using VelorenPort.Network;

namespace Server.Tests;

public class NpcAiSystemTests
{
    [Fact]
    public void Update_AttacksNearbyClient()
    {
        var em = new EntityManager();
        var npcEnt = StateExt.CreateNpc(em, new float3(0,0,0), "goblin");

        var participant = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(1), Guid.NewGuid(), null, null, null })!;
        var client = (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { participant })!;
        client.SetPosition(new float3(1,0,0));

        NpcAiSystem.Update(em, new[] { client }, 1f);

        Assert.True(client.Health < 100f);
    }
}
