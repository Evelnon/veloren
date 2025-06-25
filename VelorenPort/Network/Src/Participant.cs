using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents a remote participant with multiple channels.
    /// </summary>
    public class Participant {
        public Pid Id { get; }
        private readonly ConcurrentDictionary<Sid, Channel> _channels = new();
        private readonly ConcurrentDictionary<Sid, Stream> _streams = new();
        private readonly ConcurrentQueue<Stream> _incomingStreams = new();
        private readonly SemaphoreSlim _streamSignal = new(0);
        private readonly ConcurrentQueue<ParticipantEvent> _events = new();
        private readonly SemaphoreSlim _eventSignal = new(0);
        private float _bandwidth;

        internal Participant(Pid id) {
            Id = id;
            _bandwidth = 0f;
        }

        /// <summary>Returns the identifier of the remote participant.</summary>
        public Pid RemotePid => Id;

        /// <summary>Approximation of the available bandwidth.</summary>
        public float Bandwidth => _bandwidth;

        public Channel OpenChannel(Sid id, StreamParams parameters) {
            var ch = new Channel(id);
            _channels[id] = ch;
            _events.Enqueue(new ParticipantEvent.ChannelCreated(new ConnectAddr.Mpsc(id.Value)));
            _eventSignal.Release();
            return ch;
        }

        public Task<Stream> OpenStreamAsync(Sid id, Promises promises) {
            var stream = new Stream(id, promises);
            _streams[id] = stream;
            _incomingStreams.Enqueue(stream);
            _streamSignal.Release();
            return Task.FromResult(stream);
        }

        public async Task<Stream> OpenedAsync() {
            await _streamSignal.WaitAsync();
            _incomingStreams.TryDequeue(out var stream);
            return stream!;
        }

        internal void NotifyBandwidth(float value) => _bandwidth = value;

        public bool TryGetChannel(Sid id, out Channel channel) => _channels.TryGetValue(id, out channel);
        internal void CloseChannel(Sid id) {
            if (_channels.TryRemove(id, out _)) {
                _events.Enqueue(new ParticipantEvent.ChannelDeleted(new ConnectAddr.Mpsc(id.Value)));
                _eventSignal.Release();
            }
        }

        public async Task<ParticipantEvent> FetchEventAsync() {
            await _eventSignal.WaitAsync();
            _events.TryDequeue(out var ev);
            return ev!;
        }

        public ParticipantEvent? TryFetchEvent() {
            if (_eventSignal.Wait(0)) {
                _events.TryDequeue(out var ev);
                return ev!;
            }
            return null;
        }
    }
}
