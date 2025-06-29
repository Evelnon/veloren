using System.Linq;
using Unity.Entities;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server.Events;
using VelorenPort.Server.Sys;
using VelorenPort.NativeMath;

namespace Server.Tests;

public class ItemMergeTests
{
    [Fact]
    public void Update_MergesNearbyItems()
    {
        var em = new EntityManager();
        var e1 = em.CreateEntity();
        var e2 = em.CreateEntity();
        em.AddComponentData(e1, new Pos(new float3(0, 0, 0)));
        em.AddComponentData(e2, new Pos(new float3(1, 0, 0)));
        em.AddComponentData(e1, new PickupItem("wood", 1, true));
        em.AddComponentData(e2, new PickupItem("wood", 2, true));

        var events = new EventManager();
        ObjectSystem.Update(events, em);

        var items = em.GetEntitiesWith<PickupItem>().ToList();
        Assert.Single(items);
        var item = em.GetComponentData<PickupItem>(items[0]);
        Assert.Equal((uint)3, item.Amount);
    }
}
