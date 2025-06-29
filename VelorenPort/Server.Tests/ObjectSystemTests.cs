using System;
using Unity.Entities;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server.Sys;
using VelorenPort.Server.Events;

namespace Server.Tests;

public class ObjectSystemTests
{
    [Fact]
    public void Update_RemovesExpiredObjects()
    {
        var em = new EntityManager();
        var e = em.CreateEntity();
        var obj = Object.DeleteAfter(TimeSpan.FromSeconds(0));
        obj.SpawnedAt = DateTime.UtcNow - TimeSpan.FromSeconds(1);
        em.AddComponentData(e, obj);

        var events = new EventManager();
        ObjectSystem.Update(events, em);

        Assert.False(em.Exists(e));
    }
}
