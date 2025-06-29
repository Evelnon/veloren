using System.Collections.Generic;
using System.Reflection;
using Unity.Entities;
using VelorenPort.NativeMath;
using VelorenPort.Server.Sys;
using VelorenPort.Server;

namespace Server.Tests;

public class NpcSpawnerSystemTests
{
    [Fact]
    public void Update_SpawnsNpcWhenReady()
    {
        var em = new EntityManager();
        var sp = new NpcSpawnerSystem.SpawnPoint
        {
            Position = new float3(0,0,0),
            Interval = 1f,
            Timer = 0f,
            MaxNpcs = 1
        };
        var points = new List<NpcSpawnerSystem.SpawnPoint> { sp };
        NpcSpawnerSystem.Update(em, points, 1f);
        int count = 0;
        Entity spawned = default;
        foreach (var e in em.GetEntitiesWith<Npc>())
        {
            count++;
            spawned = e;
        }
        Assert.Equal(1, count);
        Assert.True(NpcAiSystem.IsRegistered(spawned));
    }
}
