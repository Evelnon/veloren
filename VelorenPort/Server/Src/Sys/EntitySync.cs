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

        public EntityState(Pid id, float3 position) {
            Id = id;
            Position = position;
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
        public static async Task BroadcastAsync(IEnumerable<Client> clients) {
            var states = clients
                .Select(c => new EntityState(c.Participant.Id, c.Position.Value))
                .ToArray();
            if (states.Length == 0) return;

            var msg = PreparedMsg.Create(
                0,
                new EntitySyncMessage(states),
                new StreamParams(Promises.Ordered)
            );

            var tasks = clients.Select(c => c.SendPreparedAsync(msg));
            await Task.WhenAll(tasks);
        }
    }
}
