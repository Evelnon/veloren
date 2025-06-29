using System.Collections.Generic;
using Unity.Entities;
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

    public static void Update(EntityManager em)
    {
        foreach (var (owner, pet) in Pet.EnumeratePets())
        {
            if (!em.TryGetComponentData(owner, out VelorenPort.CoreEngine.Pos ownerPos) ||
                !em.TryGetComponentData(pet, out VelorenPort.CoreEngine.Pos petPos))
                continue;

            if (math.distancesq(ownerPos.Value, petPos.Value) > LostDistanceSq)
            {
                em.AddComponentData(pet, new VelorenPort.CoreEngine.Pos(ownerPos.Value));
            }
        }
    }
}
