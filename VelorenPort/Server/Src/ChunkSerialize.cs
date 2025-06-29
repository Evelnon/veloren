using System;
using System.Collections.Generic;
using Unity.Entities;
using VelorenPort.NativeMath;

namespace VelorenPort.Server {
    /// <summary>
    /// Entry describing a chunk that should be serialized for a recipient.
    /// Mirrors <c>server/src/chunk_serialize.rs</c>.
    /// </summary>
    public readonly struct ChunkSendEntry : IEquatable<ChunkSendEntry> {
        public Entity Entity { get; }
        public int2 ChunkKey { get; }

        public ChunkSendEntry(Entity entity, int2 chunkKey) {
            Entity = entity;
            ChunkKey = chunkKey;
        }

        public bool Equals(ChunkSendEntry other) =>
            Entity.Equals(other.Entity) && ChunkKey.Equals(other.ChunkKey);

        public override bool Equals(object? obj) =>
            obj is ChunkSendEntry other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Entity, ChunkKey);
    }

    /// <summary>
    /// Serialized chunk ready to be sent to clients.
    /// </summary>
    public sealed class SerializedChunk {
        public bool LossyCompression { get; }
        public PreparedMsg Msg { get; }
        public List<Entity> Recipients { get; }

        public SerializedChunk(bool lossyCompression, PreparedMsg msg, List<Entity> recipients) {
            LossyCompression = lossyCompression;
            Msg = msg;
            Recipients = recipients;
        }
    }
}
