using Unity.Entities;
using VelorenPort.CoreEngine;
using VelorenPort.Server.Sys;

namespace Server.Tests;

public class SentinelSystemTests
{
    [Fact]
    public void Update_TracksInsertionsAndRemovals()
    {
        var em = new EntityManager();
        var trackers = new SentinelSystem.Trackers();

        var e = em.CreateEntity();
        em.AddComponentData(e, new Pos());
        SentinelSystem.Update(em, trackers);
        Assert.Contains(e, trackers.Positions.Inserted);
        Assert.Empty(trackers.Positions.Removed);

        // remove component
        em.RemoveComponent<Pos>(e);
        SentinelSystem.Update(em, trackers);
        Assert.Contains(e, trackers.Positions.Removed);
    }
}
