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
    private const float ChaseSpeed = 3f;
    private const float FleeSpeed = 4f;
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

            var vel = em.TryGetComponentData(ent, out Vel v) ? v : Vel.Zero;
            npc.Tick(dt);

            // Find nearest player or pet
            float3 targetPos = default;
            float bestDist = float.MaxValue;
            Client? targetClient = null;
            foreach (var c in clients)
            {
                float dist = math.distance(c.Position.Value, pos.Value);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    targetPos = c.Position.Value;
                    targetClient = c;
                }
            }
            foreach (var (owner, pet) in Pet.EnumeratePets())
            {

                float2 dir = math.normalizesafe(rand.NextFloat2(-1f, 1f));
                var move = new float3(dir.x, 0f, dir.y) * WanderRange * dt;
                em.SetComponentData(ent, new Pos(pos.Value + move));
                var forward = new float3(dir.x, 0f, dir.y);
                if (em.HasComponent<Ori>(ent))
                    em.SetComponentData(ent, new Ori(MathUtil.LookRotation(forward, new float3(0f,1f,0f))));
                else
                    em.AddComponentData(ent, new Ori(MathUtil.LookRotation(forward, new float3(0f,1f,0f))));
            }

            // State transitions based on proximity
            if (bestDist <= AttackRange)
            {
                npc.EnterCombat();
                if (targetClient != null)
                {
                    var attack = new Attack(npc.Id, new HitInfo(targetClient.Uid, AttackDamage, DamageKind.Physical));
                    CombatUtils.Apply(targetClient, attack, null);
                }
            }
            else if (bestDist < 10f)
            {
                npc.State = npc.Health < 30f ? NpcState.Flee : NpcState.Chase;
            }
            else if (npc.State is NpcState.Chase or NpcState.Flee)
            {
                npc.State = NpcState.Patrol;
            }

            // Movement behaviour per state
            switch (npc.State)
            {
                case NpcState.Patrol:
                    {
                        float2 dir = rand.NextFloat2(-1f, 1f);
                        dir = math.lengthsq(dir) > 0f ? math.normalize(dir) : float2.zero;
                        var move = new float3(dir.x, 0f, dir.y) * WanderRange * dt;
                        pos = new Pos(pos.Value + move);
                        vel = new Vel(move / dt);
                        break;
                    }
                case NpcState.Chase:
                    {
                        float3 dir = math.normalize(targetPos - pos.Value);
                        vel = new Vel(dir * ChaseSpeed);
                        pos = new Pos(pos.Value + vel.Value * dt);
                        break;
                    }
                case NpcState.Flee:
                    {
                        float3 dir = math.normalize(pos.Value - targetPos);
                        vel = new Vel(dir * FleeSpeed);
                        pos = new Pos(pos.Value + vel.Value * dt);
                        break;
                    }
            }

            em.SetComponentData(ent, pos);
            if (em.HasComponent<Vel>(ent))
                em.SetComponentData(ent, vel);
            else
                em.AddComponentData(ent, vel);
            em.SetComponentData(ent, npc);
        }
    }
}
