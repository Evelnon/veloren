using Unity.Entities;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server {
    /// <summary>
    /// Lightweight helpers used while constructing entities on the server. This
    /// partially mirrors server/state_ext.rs from the Rust project and will be
    /// expanded as the entity component system grows.
    /// </summary>
    public static class StateExt {
        public static Entity CreateNpc(EntityManager em, float3 pos, string name) {
            var entity = em.CreateEntity();
            em.AddComponentData(entity, new Pos(pos));
            em.AddComponentData(entity, new Vel(float3.zero));
            em.AddComponentData(entity, new Npc(new Uid((uint)entity.Index)) { Name = name });
            return entity;
        }

        public static Entity CreateObject(EntityManager em, float3 pos) {
            var entity = em.CreateEntity();
            em.AddComponentData(entity, new Pos(pos));
            return entity;
        }

        public static void InitializeCharacterData(EntityManager em, Entity entity, CharacterId id) {
            if (!em.HasComponent<CharacterId>(entity))
                em.AddComponentData(entity, id);
            if (!em.HasComponent<ViewDistances>(entity))
                em.AddComponentData(entity, new ViewDistances(8));
        }
    }
}
