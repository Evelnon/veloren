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
        private readonly Metrics? _metrics;
        private readonly Pid _pid;

        internal Channel(Sid id, Pid pid, Metrics? metrics = null) {
            Id = id;
            _pid = pid;
            _metrics = metrics;
            _metrics?.ChannelCongestion(_pid, Id, 0);
        }

        public Task SendAsync(Message msg) {
            _outgoing.Enqueue(msg);
            _metrics?.ChannelSent(_pid, Id, msg.Data.Length);
            _metrics?.ChannelSentMessage(_pid, Id);
            _metrics?.ChannelCongestion(_pid, Id, _outgoing.Count);
            return Task.CompletedTask;
        }

        public Task<Message?> ReceiveAsync() {
            if (_incoming.TryDequeue(out var msg)) {
                _metrics?.ChannelReceived(_pid, Id, msg.Data.Length);
                _metrics?.ChannelReceivedMessage(_pid, Id);
                return Task.FromResult<Message?>(msg);
            }
            return Task.FromResult<Message?>(null);
        }

        internal void PushIncoming(Message msg) => _incoming.Enqueue(msg);
        internal bool TryGetOutgoing(out Message msg) {
            var result = _outgoing.TryDequeue(out msg);
            if (result)
                _metrics?.ChannelCongestion(_pid, Id, _outgoing.Count);
            return result;
        }
    }
}
