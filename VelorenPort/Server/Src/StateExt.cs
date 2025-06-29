using System;
using Unity.Entities;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;

namespace VelorenPort.Server {
    /// <summary>
    /// Lightweight helpers used while constructing entities on the server. This
    /// partially mirrors server/state_ext.rs from the Rust project and will be
    /// expanded as the entity component system grows.
    /// </summary>
    public static class StateExt {
        /// <summary>
        /// Create a blank entity with position, orientation and velocity.
        /// This mirrors the <c>create_empty</c> helper in the Rust server.
        /// </summary>
        public static Entity CreateEmpty(EntityManager em, float3 pos)
        {
            var entity = em.CreateEntity();
            em.AddComponentData(entity, new Pos(pos));
            em.AddComponentData(entity, Vel.Zero);
            em.AddComponentData(entity, Ori.Identity);
            return entity;
        }

        /// <summary>
        /// Create a simple non-player character with basic components.
        /// </summary>
        public static Entity CreateNpc(EntityManager em, float3 pos, string name)
        {
            var entity = CreateEmpty(em, pos);
            em.AddComponentData(entity, new Controller());
            em.AddComponentData(entity, new Inventory());
            em.AddComponentData(entity, new Npc(new Uid((uint)entity.Index)) { Name = name });
            return entity;
        }

        /// <summary>
        /// Create a world object at the given position. If a lifetime is
        /// provided the object will automatically expire after that time.
        /// </summary>
        public static Entity CreateObject(EntityManager em, float3 pos, TimeSpan? lifetime = null)
        {
            var entity = CreateEmpty(em, pos);
            if (lifetime.HasValue)
                em.AddComponentData(entity, Object.DeleteAfter(lifetime.Value));
            return entity;
        }

        /// <summary>
        /// Insert default components for a player that is loading a character.
        /// </summary>
        public static void InitializeCharacterData(EntityManager em, Entity entity, CharacterId id)
        {
            if (!em.HasComponent<CharacterId>(entity))
                em.AddComponentData(entity, id);

            var spawn = SpawnPoint.Default.Position;
            if (!em.HasComponent<Pos>(entity))
                em.AddComponentData(entity, new Pos(spawn));
            if (!em.HasComponent<Vel>(entity))
                em.AddComponentData(entity, Vel.Zero);
            if (!em.HasComponent<Ori>(entity))
                em.AddComponentData(entity, Ori.Identity);
            if (!em.HasComponent<Controller>(entity))
                em.AddComponentData(entity, new Controller());

            var distances = new ViewDistances(Lib.MinViewDistance, Lib.MinViewDistance);
            if (!em.HasComponent<ViewDistances>(entity))
                em.AddComponentData(entity, distances);
            if (!em.HasComponent<Presence>(entity))
                em.AddComponentData(entity, new Presence(distances, new PresenceKind.LoadingCharacter(id)));
        }

        /// <summary>
        /// Initialize an entity as a spectator.
        /// </summary>
        public static void InitializeSpectatorData(EntityManager em, Entity entity)
        {
            var spawn = SpawnPoint.Default.Position;
            if (!em.HasComponent<Pos>(entity))
                em.AddComponentData(entity, new Pos(spawn));
            if (!em.HasComponent<Vel>(entity))
                em.AddComponentData(entity, Vel.Zero);

            var distances = new ViewDistances(Lib.MinViewDistance, Lib.MinViewDistance);
            if (!em.HasComponent<ViewDistances>(entity))
                em.AddComponentData(entity, distances);
            if (!em.HasComponent<Presence>(entity))
                em.AddComponentData(entity, new Presence(distances, new PresenceKind.Spectator()));
            else
            {
                var p = em.GetComponentData<Presence>(entity);
                p.SetKind(new PresenceKind.Spectator());
                em.SetComponentData(entity, p);
            }
        }

        /// <summary>
        /// Finish loading character data by switching the presence to Character
        /// and recording the mapping in <see cref="IdMaps"/>.
        /// </summary>
        public static void UpdateCharacterData(EntityManager em, Entity entity, CharacterId id, IdMaps maps)
        {
            if (!em.HasComponent<CharacterId>(entity))
                em.AddComponentData(entity, id);

            var distances = em.HasComponent<ViewDistances>(entity)
                ? em.GetComponentData<ViewDistances>(entity)
                : new ViewDistances(Lib.MinViewDistance, Lib.MinViewDistance);

            Presence presence;
            if (em.HasComponent<Presence>(entity))
            {
                presence = em.GetComponentData<Presence>(entity);
                presence.SetKind(new PresenceKind.Character(id));
                em.SetComponentData(entity, presence);
            }
            else
            {
                presence = new Presence(distances, new PresenceKind.Character(id));
                em.AddComponentData(entity, presence);
            }

            maps.AddCharacter(id, entity);
        }
    }
}
