using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using VelorenPort.CoreEngine.comp;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Removes expired <see cref="LootOwner"/> components from entities.
/// Simplified version of <c>server/src/sys/loot.rs</c> from the Rust codebase.
/// </summary>
public static class LootSystem
{
    public static void Update(EntityManager em)
    {
        foreach (var entity in em.GetEntitiesWith<LootOwner>())
        {
            var loot = em.GetComponentData<LootOwner>(entity);
            if (loot.Expired())
            {
                em.RemoveComponent<LootOwner>(entity);
            }
        }
    }
}
