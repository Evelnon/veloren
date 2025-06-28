using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.Quic;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents a remote participant with multiple channels.
    /// </summary>
    public class Participant : IDisposable {
        public Pid Id { get; }
        public ConnectAddr ConnectedFrom { get; }
        private readonly ConcurrentDictionary<Sid, Channel> _channels = new();
        private readonly ConcurrentDictionary<Sid, int> _channelIndices = new();
        private readonly ConcurrentDictionary<Sid, Stream> _streams = new();
        private readonly ConcurrentQueue<Stream> _incomingStreams = new();
        private readonly SemaphoreSlim _streamSignal = new(0);
        private readonly ConcurrentQueue<ParticipantEvent> _events = new();
        private readonly SemaphoreSlim _eventSignal = new(0);
        private float _bandwidth;
        private long _sentBytes;
        private long _recvBytes;
        private readonly Timer _bandwidthTimer;
        public Guid Secret { get; }
        private readonly TcpClient? _tcpClient;
        private readonly QuicConnection? _quicConnection;
        private readonly UdpClient? _udpClient;
        private readonly Metrics? _metrics;

        internal Participant(
            Pid id,
            ConnectAddr connectedFrom,
            Guid secret,
            TcpClient? tcpClient = null,
            QuicConnection? quicConnection = null,
            UdpClient? udpClient = null,
            Metrics? metrics = null)
        {
            Id = id;
            ConnectedFrom = connectedFrom;
            _bandwidth = 0f;
            Secret = secret;
            _tcpClient = tcpClient;
            _quicConnection = quicConnection;
            _udpClient = udpClient;
            _metrics = metrics;
            _metrics?.ParticipantConnected(Id);
            _bandwidthTimer = new Timer(_ =>
            {
                var sent = Interlocked.Exchange(ref _sentBytes, 0);
                var recv = Interlocked.Exchange(ref _recvBytes, 0);
                NotifyBandwidth((sent + recv) / 1f);
            }, null, 1000, 1000);
        }

        /// <summary>Returns the identifier of the remote participant.</summary>
        public Pid RemotePid => Id;

        /// <summary>Approximation of the available bandwidth.</summary>
        public float Bandwidth => _bandwidth;

        public Channel OpenChannel(Sid id, StreamParams parameters) {
            var ch = new Channel(id);
            _channels[id] = ch;
            int index = _channels.Count - 1;
            _channelIndices[id] = index;
            _metrics?.ChannelConnected(Id, index, id);
            _events.Enqueue(new ParticipantEvent.ChannelCreated(new ConnectAddr.Mpsc(id.Value)));
            _eventSignal.Release();
            return ch;
        }

        public async Task<Stream> OpenStreamAsync(Sid id, StreamParams parameters) {
            Stream stream;
            if (_tcpClient != null) {
                stream = new Stream(id, parameters.Promises, _tcpClient.GetStream(), parameters.Priority, parameters.GuaranteedBandwidth, _metrics, this);
            } else if (_quicConnection != null) {
                var qs = await _quicConnection.OpenOutboundStreamAsync();
                stream = new Stream(id, parameters.Promises, qs, parameters.Priority, parameters.GuaranteedBandwidth, _metrics, this);
            } else if (_udpClient != null) {
                stream = new Stream(id, parameters.Promises, null, parameters.Priority, parameters.GuaranteedBandwidth, _metrics, this);
            } else {
                stream = new Stream(id, parameters.Promises, null, parameters.Priority, parameters.GuaranteedBandwidth, _metrics, this);
                _incomingStreams.Enqueue(stream);
                _streamSignal.Release();
            }
            _streams[id] = stream;
            _metrics?.StreamOpened(Id);
            return stream;
        }

        public async Task<Stream> OpenedAsync() {
            if (_quicConnection != null) {
                var qs = await _quicConnection.AcceptInboundStreamAsync();
                var stream = new Stream(new Sid((ulong)_streams.Count + 1), Promises.Ordered, qs, 0, 0, _metrics, this);
                _streams[stream.Id] = stream;
                _metrics?.StreamOpened(Id);
                return stream;
            }

            await _streamSignal.WaitAsync();
            _incomingStreams.TryDequeue(out var stream);
            return stream!;
        }

        internal void NotifyBandwidth(float value)
        {
            _bandwidth = value;
            _metrics?.ParticipantBandwidth(Id, value);
        }

        internal void ReportSent(int bytes) => Interlocked.Add(ref _sentBytes, bytes);
        internal void ReportReceived(int bytes) => Interlocked.Add(ref _recvBytes, bytes);

        public bool TryGetChannel(Sid id, out Channel channel) => _channels.TryGetValue(id, out channel);
        internal void CloseChannel(Sid id) {
            if (_channels.TryRemove(id, out _)) {
                if (_channelIndices.TryRemove(id, out var index))
                    _metrics?.ChannelDisconnected(Id, index);
                _events.Enqueue(new ParticipantEvent.ChannelDeleted(new ConnectAddr.Mpsc(id.Value)));
                _eventSignal.Release();
            }
        }

        internal IEnumerable<Stream> IncomingStreams() => _streams.Values;

        public bool TryGetStream(Sid id, out Stream stream) =>
            _streams.TryGetValue(id, out stream);

        public void Close()
        {
            Dispose();
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

        public void Dispose()
        {
            _metrics?.ParticipantDisconnected(Id);
            foreach (var kv in _channelIndices)
                _metrics?.ChannelDisconnected(Id, kv.Value);
            _metrics?.CleanupParticipant(Id);
            _tcpClient?.Dispose();
            _quicConnection?.Dispose();
            _udpClient?.Dispose();
            foreach (var s in _streams.Values)
            {
                s.Dispose();
            }
            _bandwidthTimer.Dispose();
        }
    }
}
