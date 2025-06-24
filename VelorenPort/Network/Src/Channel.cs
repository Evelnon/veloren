using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Simple message channel for testing. Backed by concurrent queues.
    /// </summary>
    public class Channel {
        public Sid Id { get; }
        private readonly ConcurrentQueue<Message> _incoming = new();
        private readonly ConcurrentQueue<Message> _outgoing = new();

        internal Channel(Sid id) {
            Id = id;
        }

        public Task SendAsync(Message msg) {
            _outgoing.Enqueue(msg);
            return Task.CompletedTask;
        }

        public Task<Message?> ReceiveAsync() {
            _incoming.TryDequeue(out var msg);
            return Task.FromResult(msg);
        }

        internal void PushIncoming(Message msg) => _incoming.Enqueue(msg);
        internal bool TryGetOutgoing(out Message msg) => _outgoing.TryDequeue(out msg);
    }
}
