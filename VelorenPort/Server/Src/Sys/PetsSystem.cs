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

    public static void Update(EntityManager em, float dt)
    {
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
        }
    }
}
