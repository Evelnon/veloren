using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.NativeMath;

namespace VelorenPort.Server.Sys;

/// <summary>
/// Spawns simple NPCs at fixed locations every few seconds. This only
/// mimics the very basic behaviour of the original spawner logic and
/// is meant for testing.
/// </summary>
public static class NpcSpawnerSystem
{
    public class SpawnPoint
    {
        public float3 Position;
        public float Interval;
        public float Timer;
        public int MaxNpcs;
        public readonly List<Entity> Spawned = new();
    }

    public static void Update(EntityManager em, List<SpawnPoint> points, float dt)
    {
        foreach (var sp in points)
        {
            sp.Timer -= dt;
            sp.Spawned.RemoveAll(e => !em.Exists(e));
            if (sp.Timer <= 0f && sp.Spawned.Count < sp.MaxNpcs)
            {
                sp.Timer = sp.Interval;
                var ent = StateExt.CreateNpc(em, sp.Position, "mob");
                sp.Spawned.Add(ent);
            }
        }
    }
}
