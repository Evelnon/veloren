using System;
using Unity.Entities;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server.Sys;
using VelorenPort.Server.Events;
using VelorenPort.NativeMath;

namespace Server.Tests;

public class LootSystemTests
{
    [Fact]
    public void Update_RemovesExpiredLoot()
    {
        var em = new EntityManager();
        var e = em.CreateEntity();
        var owner = new Uid(1);
        var loot = new LootOwner(owner, false) { Expiry = DateTime.UtcNow - TimeSpan.FromSeconds(1) };
        em.AddComponentData(e, loot);

        var events = new EventManager();
        LootSystem.Update(events, em);

        Assert.False(em.HasComponent<LootOwner>(e));
        var evs = events.Drain<CreateItemDropEvent>();
        Assert.Single(evs);
        Assert.Equal("", evs[0].Item);
    }

    [Fact]
    public void Update_EmitsDropEventWithPosition()
    {
        var em = new EntityManager();
        var e = em.CreateEntity();
        em.AddComponentData(e, new LootOwner(new Uid(1), false) { Expiry = DateTime.UtcNow - TimeSpan.FromSeconds(1) });
        em.AddComponentData(e, new Pos(new float3(2, 0, 0)));
        var events = new EventManager();

        LootSystem.Update(events, em);

        var evs = events.Drain<CreateItemDropEvent>();
        Assert.Single(evs);
        Assert.Equal(new float3(2, 0, 0), evs[0].Position);
        Assert.Equal("", evs[0].Item);
        Assert.Equal((uint)0, evs[0].Amount);
    }
}
