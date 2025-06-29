using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server.Events;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Removes entities with <see cref="Object"/> set to <c>DeleteAfter</c> once the
/// timeout elapses. This mirrors part of <c>server/src/sys/object.rs</c>.
/// </summary>
public static class ObjectSystem
{
    private const float MaxItemMergeDistSq = 4f; // 2 units squared

    public static void Update(EventManager events, EntityManager em)
    {
        // Spawn item drops for queued events
        foreach (var ev in events.Drain<CreateItemDropEvent>())
        {
            var drop = em.CreateEntity();
            em.AddComponentData(drop, new Pos(ev.Position));
            em.AddComponentData(drop, new PickupItem(ev.Item, ev.Amount, true));
        }

        // Delete expired objects
        foreach (var entity in em.GetEntitiesWith<Object>())
        {
            var obj = em.GetComponentData<Object>(entity);
            if (obj.Kind == ObjectKind.DeleteAfter &&
                DateTime.UtcNow - obj.SpawnedAt >= obj.Timeout)
            {
                em.DestroyEntity(entity);
            }
        }

        // Merge nearby pickup items
        var items = em.GetEntitiesWith<PickupItem>().ToList();
        var removed = new HashSet<Entity>();
        for (int i = 0; i < items.Count; i++)
        {
            var target = items[i];
            if (removed.Contains(target) || !em.Exists(target)) continue;
            var targetItem = em.GetComponentData<PickupItem>(target);
            if (DateTime.UtcNow < targetItem.NextMergeCheck) continue;

            if (!em.TryGetComponentData(target, out Pos targetPos)) continue;

            for (int j = i + 1; j < items.Count; j++)
            {
                var source = items[j];
                if (removed.Contains(source) || !em.Exists(source)) continue;
                var sourceItem = em.GetComponentData<PickupItem>(source);
                if (!sourceItem.ShouldMerge || !targetItem.CanMerge(sourceItem)) continue;
                if (!em.TryGetComponentData(source, out Pos sourcePos)) continue;
                if (math.distancesq(targetPos.Value, sourcePos.Value) > MaxItemMergeDistSq) continue;

                targetItem.Merge(ref sourceItem);
                em.SetComponentData(target, targetItem);

                // remove merged item entity
                em.DestroyEntity(source);
                removed.Add(source);
            }

            targetItem.ScheduleNextMerge();
            em.SetComponentData(target, targetItem);
        }
    }
}
