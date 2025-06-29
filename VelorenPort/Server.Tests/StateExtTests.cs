using System;
using Unity.Entities;
using VelorenPort.NativeMath;
using VelorenPort.Server;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;

namespace Server.Tests;

public class StateExtTests
{
    [Fact]
    public void CreateNpc_AddsBasicComponents()
    {
        var em = new EntityManager();
        var npc = StateExt.CreateNpc(em, new float3(1,2,3), "orc");
        Assert.True(em.HasComponent<Pos>(npc));
        Assert.True(em.HasComponent<Ori>(npc));
        Assert.True(em.HasComponent<Vel>(npc));
        Assert.True(em.HasComponent<Inventory>(npc));
        Assert.True(em.HasComponent<Npc>(npc));
    }

    [Fact]
    public void CreateObject_CreatesEntityWithPos()
    {
        var em = new EntityManager();
        var obj = StateExt.CreateObject(em, new float3(0,0,0));
        Assert.True(em.HasComponent<Pos>(obj));
    }

    [Fact]
    public void InitializeCharacterData_SetsDefaults()
    {
        var em = new EntityManager();
        var ent = em.CreateEntity();
        StateExt.InitializeCharacterData(em, ent, new CharacterId(5));
        Assert.True(em.HasComponent<CharacterId>(ent));
        Assert.True(em.HasComponent<Presence>(ent));
        var pos = em.GetComponentData<Pos>(ent);
        Assert.Equal(SpawnPoint.Default.Position, pos.Value);
    }
}
