using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.Quic;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents a remote participant with multiple channels.
    /// </summary>
    public class Participant {
        public Pid Id { get; }
        public ConnectAddr ConnectedFrom { get; }
        private readonly ConcurrentDictionary<Sid, Channel> _channels = new();
        private readonly ConcurrentDictionary<Sid, Stream> _streams = new();
        private readonly ConcurrentQueue<Stream> _incomingStreams = new();
        private readonly SemaphoreSlim _streamSignal = new(0);
        private readonly ConcurrentQueue<ParticipantEvent> _events = new();
        private readonly SemaphoreSlim _eventSignal = new(0);
        private float _bandwidth;
        private readonly TcpClient? _tcpClient;
        private readonly QuicConnection? _quicConnection;

        internal Participant(
            Pid id,
            ConnectAddr connectedFrom,
            TcpClient? tcpClient = null,
            QuicConnection? quicConnection = null)
        {
            Id = id;
            ConnectedFrom = connectedFrom;
            _bandwidth = 0f;
            _tcpClient = tcpClient;
            _quicConnection = quicConnection;
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

        public async Task<Stream> OpenStreamAsync(Sid id, Promises promises) {
            Stream stream;
            if (_tcpClient != null) {
                stream = new Stream(id, promises, _tcpClient.GetStream());
            } else if (_quicConnection != null) {
                var qs = await _quicConnection.OpenOutboundStreamAsync();
                stream = new Stream(id, promises, qs);
            } else {
                stream = new Stream(id, promises);
                _incomingStreams.Enqueue(stream);
                _streamSignal.Release();
            }
            _streams[id] = stream;
            return stream;
        }

        public async Task<Stream> OpenedAsync() {
            if (_quicConnection != null) {
                var qs = await _quicConnection.AcceptInboundStreamAsync();
                var stream = new Stream(new Sid((ulong)_streams.Count + 1), Promises.Ordered, qs);
                _streams[stream.Id] = stream;
                return stream;
            }

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
