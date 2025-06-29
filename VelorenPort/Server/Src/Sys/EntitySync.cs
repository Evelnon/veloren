using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.Network;

namespace VelorenPort.Server.Sys {
    /// <summary>
    /// Broadcasts basic entity state to all connected clients. This is a minimal
    /// version of the Rust <c>entity_sync::Sys</c> that only transmits player
    /// positions.
    /// </summary>
    [Serializable]
    public struct EntityState {
        public Pid Id;
        public float3 Position;
        public quaternion Orientation;

        public EntityState(Pid id, float3 position, quaternion orientation) {
            Id = id;
            Position = position;
            Orientation = orientation;
        }
    }

    [Serializable]
    public struct EntitySyncMessage {
        public EntityState[] Entities;

        public EntitySyncMessage(EntityState[] entities) {
            Entities = entities;
        }
    }

    public static class EntitySync {
        public static async Task BroadcastAsync(IEnumerable<Client> clients, Unity.Entities.EntityManager em) {
            var states = new List<EntityState>();
            states.AddRange(clients.Select(c => new EntityState(
                c.Participant.Id,
                c.Position.Value,
                c.Orientation.Value)));

            foreach (var ent in em.GetEntitiesWith<Npc>())
            {
                if (!em.TryGetComponentData(ent, out Pos pos))
                    continue;
                var npc = em.GetComponentData<Npc>(ent);
                em.TryGetComponentData(ent, out Ori ori);
                states.Add(new EntityState(PidFromUid(npc.Id), pos.Value, ori.Value));
            }
            if (states.Count == 0) return;

            var msg = PreparedMsg.Create(
                0,
                new EntitySyncMessage(states),
                new StreamParams(Promises.Ordered)
            );

            var tasks = clients.Select(c => c.SendPreparedAsync(msg));
            await Task.WhenAll(tasks);
        }

        private static Pid PidFromUid(Uid uid)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(uid.Value).CopyTo(bytes, 0);
            return new Pid(new Guid(bytes));
        }
    }
}
