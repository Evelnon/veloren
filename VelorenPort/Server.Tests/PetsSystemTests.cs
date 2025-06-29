using System;
using System.Reflection;
using Unity.Entities;
using Unity.Mathematics;
using VelorenPort.Server;
using VelorenPort.Server.Sys;

namespace Server.Tests;

public class PetsSystemTests
{
    [Fact]
    public void Update_TeleportsLostPetToOwner()
    {
        var em = new EntityManager();
        var owner = em.CreateEntity();
        em.AddComponentData(owner, new VelorenPort.CoreEngine.Pos(new float3(0,0,0)));
        var pet = em.CreateEntity();
        em.AddComponentData(pet, new VelorenPort.CoreEngine.Pos(new float3(300,0,0)));

        Pet.TamePet(pet, owner);

        PetsSystem.Update(em);

        Assert.True(em.TryGetComponentData(pet, out VelorenPort.CoreEngine.Pos p));
        Assert.Equal(new float3(0,0,0), p.Value);
    }
}
