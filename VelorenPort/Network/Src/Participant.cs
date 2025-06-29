using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net.Quic;
using System.Threading;
using System.Threading.Tasks;
using VelorenPort.CoreEngine;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents a remote participant with multiple channels.
    /// </summary>
    public class Participant : IDisposable {
        public Pid Id { get; }
        public ConnectAddr ConnectedFrom { get; }
        public HandshakeFeatures Features { get; }
        /// <summary>
        /// Network version reported by the remote peer.
        /// </summary>
        public uint[] RemoteVersion { get; }
        private readonly ConcurrentDictionary<Sid, Channel> _channels = new();
        private readonly ConcurrentDictionary<Sid, int> _channelIndices = new();
        private readonly ConcurrentDictionary<Sid, Stream> _streams = new();
        private readonly ConcurrentQueue<Stream>[] _incomingQueues = new ConcurrentQueue<Stream>[Stream.PriorityLevels];
        private int _currentIncomingPrio;
        private int _remainingIncomingWeight;
        private readonly SemaphoreSlim _streamSignal = new(0);
        private readonly ConcurrentQueue<ParticipantEvent> _events = new();
        private readonly SemaphoreSlim _eventSignal = new(0);
        /// <summary>
        /// Notifies listeners whenever the bandwidth estimate changes.
        /// </summary>
        public event Action<float>? BandwidthUpdated;
        /// <summary>
        /// Raised when the participant disconnects and all streams are closed.
        /// </summary>
        public event Action? Disconnected;
        /// <summary>
        /// Triggered whenever a new stream is opened.
        /// </summary>
        public event Action<Stream>? StreamOpened;
        /// <summary>
        /// Triggered when a stream is disposed and removed.
        /// </summary>
        public event Action<Stream>? StreamClosed;
        private float _bandwidth;
        private long _sentBytes;
        private long _recvBytes;
        private long _totalSentBytes;
        private long _totalRecvBytes;
        private readonly Timer _bandwidthTimer;
        public Guid Secret { get; }
        public ClientType ClientType { get; }
        private readonly TcpClient? _tcpClient;
        private readonly QuicConnection? _quicConnection;
        private readonly UdpClient? _udpClient;
        private readonly Metrics? _metrics;
        private readonly Sid _streamOffset;
        private ulong _nextSidValue;
        public Credentials Credentials { get; }
        public AdminRole? RoleRequirement { get; }

        internal Participant(
            Pid id,
            ConnectAddr connectedFrom,
            Guid secret,
            TcpClient? tcpClient = null,
            QuicConnection? quicConnection = null,
            UdpClient? udpClient = null,
            Metrics? metrics = null,
            HandshakeFeatures features = HandshakeFeatures.None,
            Sid streamOffset = default,
            uint[]? remoteVersion = null,
            ClientType? clientType = null,
            Credentials? credentials = null,
            AdminRole? roleRequirement = null)
        {
            Id = id;
            ConnectedFrom = connectedFrom;
            Features = features;
            RemoteVersion = remoteVersion ?? Array.Empty<uint>();
            ClientType = clientType ?? new ClientType.Game();
            Credentials = credentials ?? new Credentials(string.Empty);
            RoleRequirement = roleRequirement;
            if (!Credentials.IsValid)
                throw new InvalidOperationException("Invalid credentials");
            if (!ClientType.IsValidForRole(roleRequirement))
                throw new InvalidOperationException("Client type not permitted for this role");
            _bandwidth = 0f;
            Secret = secret;
            _tcpClient = tcpClient;
            _quicConnection = quicConnection;
            _udpClient = udpClient;
            _metrics = metrics;
            _streamOffset = streamOffset;
            _nextSidValue = streamOffset.Value;
            for (int i = 0; i < _incomingQueues.Length; i++)
                _incomingQueues[i] = new ConcurrentQueue<Stream>();
            _currentIncomingPrio = 0;
            _remainingIncomingWeight = Stream.GetWeight(0);
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

        /// <summary>Returns the next available stream identifier.</summary>
        public Sid NextSid()
        {
            var sid = new Sid(_nextSidValue);
            _nextSidValue++;
            return sid;
        }

        public Channel OpenChannel(Sid id, StreamParams parameters) {
            var ch = new Channel(id, Id, _metrics);
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
                var remote = (ConnectedFrom as ConnectAddr.Udp)!.EndPoint;
                stream = new Stream(id, parameters.Promises, null, parameters.Priority, parameters.GuaranteedBandwidth, _metrics, this, _udpClient, remote);
            } else {
                stream = new Stream(id, parameters.Promises, null, parameters.Priority, parameters.GuaranteedBandwidth, _metrics, this);
                EnqueueIncoming(stream);
            }
            _streams[id] = stream;
            _metrics?.StreamOpened(Id);
            _metrics?.StreamRtt(Id, id, 0);
            StreamOpened?.Invoke(stream);
            return stream;
        }

        public async Task<Stream> OpenedAsync() {
            if (_quicConnection != null) {
                var qs = await _quicConnection.AcceptInboundStreamAsync();
                var stream = new Stream(NextSid(), Promises.Ordered, qs, 0, 0, _metrics, this);
                _streams[stream.Id] = stream;
                _metrics?.StreamOpened(Id);
                _metrics?.StreamRtt(Id, stream.Id, 0);
                StreamOpened?.Invoke(stream);
                return stream;
            }

            await _streamSignal.WaitAsync();
            if (TryDequeueIncoming(out var stream))
                return stream!;
            return await OpenedAsync();
        }

        internal void NotifyBandwidth(float value)
        {
            _bandwidth = value;
            _metrics?.ParticipantBandwidth(Id, value);
            BandwidthUpdated?.Invoke(value);
        }

        internal void ReportSent(int bytes)
        {
            Interlocked.Add(ref _sentBytes, bytes);
            Interlocked.Add(ref _totalSentBytes, bytes);
        }

        internal void ReportReceived(int bytes)
        {
            Interlocked.Add(ref _recvBytes, bytes);
            Interlocked.Add(ref _totalRecvBytes, bytes);
        }

        /// <summary>
        /// Enqueue a <see cref="ParticipantEvent.GroupUpdate"/> for this
        /// participant. This allows higher layers to synchronize group state
        /// without direct access to <see cref="Client"/> instances.
        /// </summary>
        internal void NotifyGroupUpdate(CoreEngine.comp.GroupEvent ev)
        {
            _events.Enqueue(new ParticipantEvent.GroupUpdate(ev));
            _eventSignal.Release();
        }

        internal void NotifyGroupPrivilegeUpdate(CoreEngine.comp.Group group, CoreEngine.comp.Uid member, CoreEngine.comp.GroupPrivileges privs)
        {
            _events.Enqueue(new ParticipantEvent.GroupPrivilegeUpdate(group, member, privs));
            _eventSignal.Release();
        }

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

        /// <summary>Enumerates all currently open streams.</summary>
        public IEnumerable<Stream> Streams => _streams.Values;

        /// <summary>Enumerates all currently open channels.</summary>
        public IEnumerable<Channel> Channels => _channels.Values;

        public bool TryGetStream(Sid id, out Stream stream) =>
            _streams.TryGetValue(id, out stream);

        private void EnqueueIncoming(Stream stream)
        {
            byte p = stream.Priority >= Stream.PriorityLevels ? (byte)(Stream.PriorityLevels - 1) : stream.Priority;
            _incomingQueues[p].Enqueue(stream);
            _streamSignal.Release();
        }

        private bool TryDequeueIncoming(out Stream stream)
        {
            for (int i = 0; i < _incomingQueues.Length; i++)
            {
                int prio = (_currentIncomingPrio + i) % _incomingQueues.Length;
                if (_incomingQueues[prio].TryDequeue(out stream))
                {
                    _remainingIncomingWeight--;
                    if (_remainingIncomingWeight <= 0)
                    {
                        _currentIncomingPrio = (prio + 1) % _incomingQueues.Length;
                        _remainingIncomingWeight = Stream.GetWeight(_currentIncomingPrio);
                    }
                    return true;
                }
            }
            _currentIncomingPrio = (_currentIncomingPrio + 1) % _incomingQueues.Length;
            _remainingIncomingWeight = Stream.GetWeight(_currentIncomingPrio);
            stream = null!;
            return false;
        }

        internal void RemoveStream(Sid id)
        {
            if (_streams.TryRemove(id, out var s))
            {
                StreamClosed?.Invoke(s);
            }
        }

        /// <summary>
        /// Obtiene las estadísticas de tráfico acumuladas para este participante.
        /// </summary>
        public (long sentBytes, long recvBytes) StatsSnapshot() =>
            (Interlocked.Read(ref _totalSentBytes), Interlocked.Read(ref _totalRecvBytes));

        public void Close()
        {
            Dispose();
        }

        public async Task DisconnectAsync()
        {
            foreach (var stream in _streams.Values)
            {
                await stream.CloseAsync();
                stream.Dispose();
            }
            _streams.Clear();
            foreach (var id in _channels.Keys)
                CloseChannel(id);
            _tcpClient?.Dispose();
            _quicConnection?.Dispose();
            _udpClient?.Dispose();
            _metrics?.ParticipantDisconnected(Id);
            _metrics?.CleanupParticipant(Id);
            Disconnected?.Invoke();
            await Task.CompletedTask;
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
            Disconnected?.Invoke();
            _bandwidthTimer.Dispose();
        }
    }
}
