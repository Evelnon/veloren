using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace VelorenPort.Server.Sys {
    /// <summary>
    /// Simplified system that forwards serialized chunks to all connected
    /// clients. Mirrors the behaviour of <c>chunk_send.rs</c> in the Rust
    /// server, minus entity filtering and compression management.
    /// </summary>
    public static class ChunkSend {
        public static async Task FlushAsync(
            Channel<SerializedChunk> channel,
            IEnumerable<Client> clients,
            NetworkRequestMetrics metrics) {
            ulong lossy = 0;
            ulong lossless = 0;
            var reader = channel.Reader;
            var sendTasks = new List<Task>();
            while (reader.TryRead(out var chunk)) {
                foreach (var client in clients) {
                    sendTasks.Add(client.SendPreparedAsync(chunk.Msg));
                }
                if (chunk.LossyCompression) lossy++; else lossless++;
            }
            if (sendTasks.Count > 0)
                await Task.WhenAll(sendTasks);
            metrics.ChunksServedLossy.IncBy((long)lossy);
            metrics.ChunksServedLossless.IncBy((long)lossless);
        }
    }
}
