using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents a unidirectional stream between two participants.
    /// </summary>
    public class Stream {
        public Sid Id { get; }
        public Promises Promises { get; }
        private readonly ConcurrentQueue<Message> _rx = new();
        private readonly ConcurrentQueue<Message> _tx = new();

        internal Stream(Sid id, Promises promises) {
            Id = id;
            Promises = promises;
        }

        public Task SendAsync(Message msg) {
            _tx.Enqueue(msg);
            return Task.CompletedTask;
        }

        public Task<Message?> RecvAsync() {
            _rx.TryDequeue(out var msg);
            return Task.FromResult(msg);
        }

        internal void PushIncoming(Message msg) => _rx.Enqueue(msg);
        internal bool TryDequeueOutgoing(out Message msg) => _tx.TryDequeue(out msg);
    }
}
