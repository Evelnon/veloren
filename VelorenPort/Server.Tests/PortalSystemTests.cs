using System;
using System.Reflection;
using Unity.Entities;
using Unity.Mathematics;
using VelorenPort.Server.Sys;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server;
using VelorenPort.Network;

namespace Server.Tests;

public class PortalSystemTests
{
    [Fact]
    public void Update_TeleportsAfterBuildup()
    {
        var em = new EntityManager();
        var portal = em.CreateEntity();
        em.AddComponentData(portal, new Pos(new float3(0,0,0)));
        em.AddComponentData(portal, Object.Portal(new float3(10,0,0), false, 0.5f));

        var participant = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(1), Guid.NewGuid(), null, null, null })!;
        var client = (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { participant })!;
        client.SetPosition(new float3(0,0,0));

        PortalSystem.Update(em, new[] { client }, 0.6f);

        Assert.Equal(new float3(10,0,0), client.Position.Value);
    }
}
