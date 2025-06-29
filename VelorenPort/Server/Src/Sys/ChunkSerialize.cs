using System.Collections.Generic;
using System.Threading.Channels;
using Unity.Entities;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.World;

namespace VelorenPort.Server.Sys {
    /// <summary>
    /// Collects chunk serialization requests and serializes them into
    /// <see cref="SerializedChunk"/> messages. This mirrors the behaviour of
    /// <c>chunk_serialize.rs</c> in a simplified form.
    /// </summary>
    public class ChunkSerialize {
        private readonly EventBus<ChunkSendEntry> _bus = new();

        public IEmitExt<ChunkSendEntry> GetEmitter() => _bus.GetEmitter();

        public void Queue(ChunkSendEntry entry) => _bus.EmitNow(entry);

        public void Queue(int2 chunkKey) => _bus.EmitNow(new ChunkSendEntry(new Entity(), chunkKey));

        public void Flush(
            WorldIndex index,
            Channel<SerializedChunk> channel,
            NetworkRequestMetrics metrics)
        {
            var events = _bus.RecvAll();
            if (events.Count == 0) return;

            ulong requests = 0;
            ulong distinct = 0;
            var grouped = new Dictionary<int2, List<Entity>>();

            foreach (var ev in events) {
                requests++;
                if (!grouped.TryGetValue(ev.ChunkKey, out var list)) {
                    list = new List<Entity>();
                    grouped[ev.ChunkKey] = list;
                    distinct++;
                }
                list.Add(ev.Entity);
            }

            metrics.ChunksSerialisationRequests.IncBy((long)requests);
            metrics.ChunksDistinctSerialisationRequests.IncBy((long)distinct);

            foreach (var (key, recipients) in grouped) {
                var chunk = index.Map.GetOrGenerate(key, index.Noise);
                var msg = PreparedMsg.Create(
                    0,
                    chunk,
                    new StreamParams(Promises.Ordered));
                channel.Writer.TryWrite(new SerializedChunk(false, msg, recipients));
            }
        }
    }
}
