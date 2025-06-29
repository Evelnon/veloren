using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.Server.Agent;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Behaviour tree based AI system for NPCs. This is a small subset of the
/// original Rust agent crate implemented for testing.
/// </summary>
public static class NpcAiSystem
{
    private static readonly Dictionary<Entity, BehaviorTree> Trees = new();

    public static void RegisterNpc(EntityManager em, Entity entity)
    {
        Trees[entity] = NpcBehaviorFactory.CreateDefault();
    }

    public static void UnregisterNpc(Entity entity)
    {
        Trees.Remove(entity);
    }

    public static bool IsRegistered(Entity entity) => Trees.ContainsKey(entity);

    public static void Update(EntityManager em, IEnumerable<Client> clients, float dt, float worldTime)
    {
        bool isNight = worldTime % 86400f >= 43200f;

        foreach (var ent in em.GetEntitiesWith<Npc>())
        {
            if (!Trees.TryGetValue(ent, out var tree))
            {
                tree = NpcBehaviorFactory.CreateDefault();
                Trees[ent] = tree;
            }

            var npc = em.GetComponentData<Npc>(ent);
            if (isNight && npc.State == NpcState.Patrol)
                npc.State = NpcState.Idle;
            tree.Tick(em, ent, npc, clients, dt);
            em.SetComponentData(ent, npc);
        }
    }
}
