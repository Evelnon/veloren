using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Ensures pets stay near their owner. Any pet that strays too far
/// is teleported back to the owner's position. This roughly mirrors
/// <c>server/src/sys/pets.rs</c> in a greatly simplified manner.
/// </summary>
public static class PetsSystem
{
    private const float LostDistanceSq = 200f * 200f;
    private const float FollowDistanceSq = 25f; // 5 units
    private const float FollowSpeed = 5f;       // units per second

    private const float AttackRange = 2f;
    private const float AttackDamage = 5f;
    private const float AttackCooldown = 1f;

    private static readonly Dictionary<Entity, float> _cooldowns = new();

    public static void Update(EntityManager em, IEnumerable<Client> clients, float dt)
    {
        // Decrease cooldown timers and remove entries for destroyed pets
        var remove = new List<Entity>();
        foreach (var (pet, time) in _cooldowns)
        {
            if (!em.Exists(pet))
            {
                remove.Add(pet);
                continue;
            }
            _cooldowns[pet] = math.max(0f, time - dt);
        }
        foreach (var p in remove)
            _cooldowns.Remove(p);

        foreach (var (owner, pet) in Pet.EnumeratePets())
        {
            if (!em.TryGetComponentData(owner, out VelorenPort.CoreEngine.Pos ownerPos) ||
                !em.TryGetComponentData(pet, out VelorenPort.CoreEngine.Pos petPos))
                continue;

            float3 ownerV = ownerPos.Value;
            float3 petV = petPos.Value;
            float distSq = math.distancesq(ownerV, petV);

            if (distSq > LostDistanceSq)
            {
                em.SetComponentData(pet, new VelorenPort.CoreEngine.Pos(ownerV));
                continue;
            }

            if (distSq > FollowDistanceSq)
            {
                float3 dir = math.normalizesafe(ownerV - petV);
                float3 move = dir * FollowSpeed * dt;
                em.SetComponentData(pet, new VelorenPort.CoreEngine.Pos(petV + move));

                var rot = MathUtil.LookRotation(dir, new float3(0f, 1f, 0f));
                if (em.HasComponent<Ori>(pet))
                    em.SetComponentData(pet, new Ori(rot));
                else
                    em.AddComponentData(pet, new Ori(rot));
            }

            // Combat: attack nearby hostile NPCs and players
            if (!_cooldowns.TryGetValue(pet, out var cd))
                cd = 0f;
            if (cd <= 0f)
            {
                bool attacked = false;

                // Attack NPCs
                foreach (var target in em.GetEntitiesWith<Npc>())
                {
                    if (target.Equals(pet))
                        continue;
                    if (!em.TryGetComponentData(target, out Pos tpos))
                        continue;
                    if (math.distance(tpos.Value, petV) <= AttackRange)
                    {
                        var npcData = em.GetComponentData<Npc>(target);
                        npcData.TakeDamage(AttackDamage);
                        em.SetComponentData(target, npcData);
                        attacked = true;
                        break;
                    }
                }

                if (!attacked)
                {
                    foreach (var c in clients)
                    {
                        if (math.distance(c.Position.Value, petV) <= AttackRange)
                        {
                            var attack = new Attack(new Uid((uint)pet.Index), new HitInfo(c.Uid, AttackDamage, DamageKind.Physical));
                            CombatUtils.Apply(c, attack, null);
                            attacked = true;
                            break;
                        }
                    }
                }

                if (attacked)
                    _cooldowns[pet] = AttackCooldown;
            }
            else
            {
                _cooldowns[pet] = cd;
            }
        }
    }
}
