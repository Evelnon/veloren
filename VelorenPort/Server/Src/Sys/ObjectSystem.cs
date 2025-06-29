using System;
using Unity.Collections;
using Unity.Entities;
using VelorenPort.CoreEngine.comp;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Removes entities with <see cref="Object"/> set to <c>DeleteAfter</c> once the
/// timeout elapses. This mirrors part of <c>server/src/sys/object.rs</c>.
/// </summary>
public static class ObjectSystem
{
    public static void Update(EntityManager em)
    {
        foreach (var entity in em.GetEntitiesWith<Object>())
        {
            var obj = em.GetComponentData<Object>(entity);
            if (obj.Kind == ObjectKind.DeleteAfter &&
                DateTime.UtcNow - obj.SpawnedAt >= obj.Timeout)
            {
                em.DestroyEntity(entity);
            }
        }
    }
}
