using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server.Events;
using VelorenPort.NativeMath;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Removes expired <see cref="LootOwner"/> components from entities.
/// Simplified version of <c>server/src/sys/loot.rs</c> from the Rust codebase.
/// </summary>
public static class LootSystem
{
    public static void Update(EventManager events, EntityManager em)
    {
        using var emitter = events.GetEmitter<CreateItemDropEvent>();
        foreach (var entity in em.GetEntitiesWith<LootOwner>())
        {
            var loot = em.GetComponentData<LootOwner>(entity);
            if (loot.Expired())
            {
                em.RemoveComponent<LootOwner>(entity);
                if (em.TryGetComponentData(entity, out Pos pos))
                    emitter.Emit(new CreateItemDropEvent(pos.Value, string.Empty, 0));
                else
                    emitter.Emit(new CreateItemDropEvent(new float3(0f, 0f, 0f), string.Empty, 0));
            }
        }
    }
}
