using System;
using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Extremely lightweight AI system for NPCs. NPCs wander around and
/// deal a small amount of damage to nearby players. This only covers
/// a tiny subset of the behaviour of the original Rust agent system.
/// </summary>
public static class NpcAiSystem
{
    private const float WanderRange = 1f;
    private const float AttackRange = 2f;
    private const float AttackDamage = 5f;

    public static void Update(EntityManager em, IEnumerable<Client> clients, float dt)
    {
        var rand = new Random((uint)Environment.TickCount);
        foreach (var ent in em.GetEntitiesWith<Npc>())
        {
            var npc = em.GetComponentData<Npc>(ent);
            if (!em.TryGetComponentData(ent, out Pos pos))
                continue;

            npc.Tick(dt);
            if (npc.State == NpcState.Wandering)
            {
                float2 dir = math.normalizesafe(rand.NextFloat2(-1f, 1f));
                var move = new float3(dir.x, 0f, dir.y) * WanderRange * dt;
                em.SetComponentData(ent, new Pos(pos.Value + move));
            }

            foreach (var c in clients)
            {
                if (math.distance(c.Position.Value, pos.Value) <= AttackRange)
                {
                    npc.EnterCombat();
                    var attack = new Attack(npc.Id, new HitInfo(c.Uid, AttackDamage, DamageKind.Physical));
                    CombatUtils.Apply(c, attack, null);
                }
            }

            em.SetComponentData(ent, npc);
        }
    }
}
