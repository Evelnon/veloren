using System;
using Unity.Entities;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server.Sys;

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

        LootSystem.Update(em);

        Assert.False(em.HasComponent<LootOwner>(e));
    }
}
