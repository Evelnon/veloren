using System;
using System.Collections.Generic;
using System.Reflection;
using VelorenPort.NativeMath;
using VelorenPort.Server;
using VelorenPort.Server.Sys;
using VelorenPort.Server.Rtsim;
using VelorenPort.World;

namespace Server.Tests;

public class TerrainSyncIntegrationTests
{
    private static Client CreateClient()
    {
        var participant = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(1), Guid.NewGuid(), null, null, null })!;
        return (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { participant })!;
    }

    [Fact]
    public void Update_LoadsSupplementData()
    {
        var index = new WorldIndex(0);
        var client = CreateClient();
        var serialize = new ChunkSerialize();
        var spawns = new List<NpcSpawnerSystem.SpawnPoint>();
        var sim = new RtSim();

        TerrainSync.Update(index, client, serialize, spawns, sim);

        var sup = index.Map.GetSupplement(new int2(0,0));
        Assert.NotNull(sup);
        Assert.Equal(sup!.SpawnPoints.Count, spawns.Count);
        Assert.Equal(sup.ResourceDeposits.Count, sim.ResourceCounter);
    }
}

