using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Manejador de red independiente de Unity. Mantiene una lista de participantes
    /// conectados y permite iniciar futuras conexiones mediante sockets.
    /// </summary>
    public class Network {
        public Pid LocalPid { get; }
        private readonly ConcurrentQueue<Participant> _pending = new();
        private readonly ConcurrentDictionary<Pid, Participant> _participants = new();
        private readonly ConcurrentDictionary<IPEndPoint, Participant> _udpMap = new();
        private static readonly ConcurrentDictionary<ulong, Network> _mpscListeners = new();
        private TcpListener? _tcpListener;
        private QuicListener? _quicListener;
        private UdpClient? _udpListener;
        private CancellationTokenSource? _listenCts;
        private readonly Guid _localSecret = Guid.NewGuid();
        private HandshakeFeatures _features = HandshakeFeatures.None;
        public Metrics Metrics { get; }

        /// <summary>
        /// Raised whenever a participant successfully connects.
        /// </summary>
        public event Action<Participant>? ParticipantConnected;

        /// <summary>
        /// Raised after a participant has been fully disconnected and removed.
        /// </summary>
        public event Action<Pid>? ParticipantDisconnected;

        public Network(Pid pid) {
            LocalPid = pid;
            Metrics = new Metrics(pid);
        }

        public void StartMetrics(int port = 9091) => Metrics.StartPrometheus(port);
        public void StopMetrics() => Metrics.StopPrometheus();

        public Task ListenAsync(ListenAddr addr, HandshakeFeatures features = HandshakeFeatures.None) {
            Metrics.ListenRequest(addr);
            _listenCts?.Cancel();
            _listenCts = new CancellationTokenSource();
            _features = features;

            switch (addr) {
                case ListenAddr.Tcp tcp:
                    _tcpListener = new TcpListener(tcp.EndPoint);
                    _tcpListener.Start();
                    _ = AcceptTcpAsync(_listenCts.Token);
                    break;
                case ListenAddr.Udp udp:
                    _udpListener = new UdpClient(udp.EndPoint);
                    _ = AcceptUdpAsync(_listenCts.Token);
                    break;
                case ListenAddr.Quic quic:
                    var options = new QuicListenerOptions {
                        ListenEndPoint = quic.EndPoint,
                        ApplicationProtocols = new[] { new SslApplicationProtocol("veloren") },
                        ConnectionOptionsCallback = (_, _) => new ValueTask<QuicServerConnectionOptions>(
                            new QuicServerConnectionOptions {
                                DefaultStreamErrorCode = 0,
                                ServerAuthenticationOptions = new SslServerAuthenticationOptions {
                                    ServerCertificate = new X509Certificate2(quic.Config.CertificatePath, quic.Config.PrivateKeyPath)
                                }
                            })
                    };
                    _quicListener = new QuicListener(options);
                    _quicListener.Start();
                    _ = AcceptQuicAsync(_listenCts.Token);
                    break;
                case ListenAddr.Mpsc mpsc:
                    _mpscListeners[mpsc.ChannelId] = this;
                    break;
                default:
                    throw new NotSupportedException("Unsupported listen address");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Detiene todas las escuchas de sockets activos.
        /// </summary>
        public void StopListening()
        {
            _listenCts?.Cancel();
            _tcpListener?.Stop();
            _quicListener?.Dispose();
            _udpListener?.Dispose();
            _listenCts = null;
            _tcpListener = null;
            _quicListener = null;
            _udpListener = null;
        }

        public async Task<Participant> ConnectAsync(ConnectAddr addr, HandshakeFeatures features = HandshakeFeatures.None) {
            Metrics.ConnectRequest(addr);
            switch (addr) {
                case ConnectAddr.Tcp tcp:
                    var client = new TcpClient();
                    await client.ConnectAsync(tcp.EndPoint.Address, tcp.EndPoint.Port);
                    try {
                        var (rpid, rsec, rfeat, rver, offset) = await Handshake.PerformAsync(client.GetStream(), true, LocalPid, _localSecret, features);
                        var agreed = rfeat & features;
                        var p = new Participant(rpid, addr, rsec, client, null, null, Metrics, agreed, offset, rver);
                        _participants[p.Id] = p;
                        _pending.Enqueue(p);
                        ParticipantConnected?.Invoke(p);
                        return p;
                    } catch {
                        Metrics.FailedHandshake();
                        client.Dispose();
                        throw;
                    }
                case ConnectAddr.Udp udp:
                    var u = new UdpClient();
                    await u.ConnectAsync(udp.EndPoint);
                    try {
                        var (upid, usecret, ufeat, uver, offset) = await SendUdpHandshakeAsync(u, udp.EndPoint, LocalPid, _localSecret, features);
                        var uagreed = ufeat & features;
                        var up = new Participant(upid, addr, usecret, null, null, u, Metrics, uagreed, offset, uver);
                        _participants[up.Id] = up;
                        _pending.Enqueue(up);
                        ParticipantConnected?.Invoke(up);
                        return up;
                    } catch {
                        Metrics.FailedHandshake();
                        u.Dispose();
                        throw;
                    }
                case ConnectAddr.Quic quic:
                    var options = new QuicClientConnectionOptions {
                        RemoteEndPoint = quic.EndPoint,
                        ClientAuthenticationOptions = new SslClientAuthenticationOptions {
                            RemoteCertificateValidationCallback = (s, c, ch, e) => quic.Config.InsecureSkipVerify || e == SslPolicyErrors.None,
                            ApplicationProtocols = new[] { new SslApplicationProtocol(quic.Name) }
                        }
                    };
                    var conn = new QuicConnection(options);
                    await conn.ConnectAsync();
                    var hs = await conn.OpenOutboundStreamAsync();
                    try {
                        var (qpid, qsec, qfeat, qver, offset) = await Handshake.PerformAsync(hs, true, LocalPid, _localSecret, features);
                        await hs.DisposeAsync();
                        var qagreed = qfeat & features;
                        var qp = new Participant(qpid, addr, qsec, null, conn, null, Metrics, qagreed, offset, qver);
                        _participants[qp.Id] = qp;
                        _pending.Enqueue(qp);
                        ParticipantConnected?.Invoke(qp);
                        return qp;
                    } catch {
                        Metrics.FailedHandshake();
                        await hs.DisposeAsync();
                        await conn.DisposeAsync();
                        throw;
                    }
                case ConnectAddr.Mpsc mpsc:
                    if (!_mpscListeners.TryGetValue(mpsc.ChannelId, out var remote))
                        throw new NetworkConnectError.Io(new InvalidOperationException("no listener"));
                    var secret = Guid.NewGuid();
                    var remoteParticipant = new Participant(LocalPid, new ConnectAddr.Mpsc(mpsc.ChannelId), secret, null, null, null, remote.Metrics, features, Types.STREAM_ID_OFFSET2, Handshake.SupportedVersion);
                    var localParticipant = new Participant(remote.LocalPid, addr, secret, null, null, null, Metrics, features, Types.STREAM_ID_OFFSET1, Handshake.SupportedVersion);
                    var ls = await localParticipant.OpenStreamAsync(localParticipant.NextSid(), new StreamParams(Promises.Ordered));
                    var rs = await remoteParticipant.OpenStreamAsync(remoteParticipant.NextSid(), new StreamParams(Promises.Ordered));
                    StartMpscRelay(ls, rs);
                    _participants[localParticipant.Id] = localParticipant;
                    remote._participants[remoteParticipant.Id] = remoteParticipant;
                    _pending.Enqueue(localParticipant);
                    ParticipantConnected?.Invoke(localParticipant);
                    remote._pending.Enqueue(remoteParticipant);
                    remote.ParticipantConnected?.Invoke(remoteParticipant);
                    return localParticipant;
                default:
                    throw new NotSupportedException("Unsupported connect address");
            }
        }

        public Task<Participant?> ConnectedAsync() {
            _pending.TryDequeue(out var participant);
            return Task.FromResult(participant);
        }

        public bool TryGetParticipant(Pid id, out Participant participant)
            => _participants.TryGetValue(id, out participant);

        private async Task AcceptTcpAsync(CancellationToken token) {
            if (_tcpListener == null) return;
            while (!token.IsCancellationRequested) {
                var client = await _tcpListener.AcceptTcpClientAsync(token);
                var ep = (IPEndPoint)client.Client.RemoteEndPoint!;
                Metrics.IncomingConnection(new ConnectAddr.Tcp(ep));
                var (pid, secret, feat, ver, offset) = await Handshake.PerformAsync(client.GetStream(), false, LocalPid, _localSecret, _features, token);
                var agreed = feat & _features;
                var participant = new Participant(pid, new ConnectAddr.Tcp(ep), secret, client, null, null, Metrics, agreed, offset, ver);
                _participants[participant.Id] = participant;
                _pending.Enqueue(participant);
                ParticipantConnected?.Invoke(participant);
            }
        }

        private async Task AcceptUdpAsync(CancellationToken token) {
            if (_udpListener == null) return;
            while (!token.IsCancellationRequested) {
                var result = await _udpListener.ReceiveAsync(token);
                var remote = result.RemoteEndPoint;
                Metrics.IncomingConnection(new ConnectAddr.Udp(remote));
                if (!_udpMap.TryGetValue(remote, out var participant)) {
                    if (Handshake.TryParse(result.Buffer, out var pid, out var secret, out var flags, out var ver)) {
                        var reply = Handshake.GetBytes(LocalPid, _localSecret, _features);
                        _udpListener.SendAsync(reply, reply.Length, remote).ConfigureAwait(false);
                        var agreed = flags & _features;
                        participant = new Participant(pid, new ConnectAddr.Udp(remote), secret, null, null, _udpListener, Metrics, agreed, Types.STREAM_ID_OFFSET2, ver);
                        _participants[participant.Id] = participant;
                        _udpMap[remote] = participant;
                        _pending.Enqueue(participant);
                        ParticipantConnected?.Invoke(participant);
                        await participant.OpenStreamAsync(participant.NextSid(), new StreamParams(Promises.Ordered));
                    } else {
                        Metrics.FailedHandshake();
                    }
                } else {
                    var data = result.Buffer;
                    if (data.Length >= 17) {
                        ulong sid = BitConverter.ToUInt64(data, 0);
                        byte kind = data[8];
                        ulong mid = BitConverter.ToUInt64(data, 9);
                        var payload = new byte[data.Length - 17];
                        if (payload.Length > 0)
                            Buffer.BlockCopy(data, 17, payload, 0, payload.Length);
                        if (participant.TryGetStream(new Sid(sid), out var s))
                            s.ProcessDatagram(kind, mid, payload);
                    }
                }
            }
        }

        private async Task AcceptQuicAsync(CancellationToken token) {
            if (_quicListener == null) return;
            while (!token.IsCancellationRequested) {
                var connection = await _quicListener.AcceptConnectionAsync(token);
                var ep = connection.RemoteEndPoint as IPEndPoint ?? new IPEndPoint(IPAddress.Any, 0);
                Metrics.IncomingConnection(new ConnectAddr.Quic(ep, new QuicClientConfig(), "quic"));
                var hs = await connection.AcceptInboundStreamAsync(token);
                try {
                    var (pid, secret, feat, ver, offset) = await Handshake.PerformAsync(hs, false, LocalPid, _localSecret, _features, token);
                    await hs.DisposeAsync();
                    var agreed = feat & _features;
                    var participant = new Participant(pid, new ConnectAddr.Quic(ep, new QuicClientConfig(), "quic"), secret, null, connection, null, Metrics, agreed, offset, ver);
                    _participants[participant.Id] = participant;
                    _pending.Enqueue(participant);
                    ParticipantConnected?.Invoke(participant);
                } catch {
                    Metrics.FailedHandshake();
                    await hs.DisposeAsync();
                    await connection.DisposeAsync();
                }
            }
        }


        private static async Task<(Pid pid, Guid secret, HandshakeFeatures features, uint[] version, Sid offset)> SendUdpHandshakeAsync(UdpClient client, IPEndPoint remote, Pid localPid, Guid localSecret, HandshakeFeatures features) {
            var buffer = Handshake.GetBytes(localPid, localSecret, features);
            await client.SendAsync(buffer, buffer.Length);
            var result = await client.ReceiveAsync();
            if (!Handshake.TryParse(result.Buffer, out var pid, out var secret, out var features, out var version))
                throw new NetworkConnectError.Handshake(new InitProtocolError<ProtocolsError>.NotHandshake());
            return (pid, secret, features, version, Types.STREAM_ID_OFFSET1);
        }

        private static void StartMpscRelay(Stream a, Stream b) {
            _ = Task.Run(async () => {
                while (true) {
                    var moved = false;
                    while (a.TryDequeueOutgoing(out var am)) {
                        b.PushIncoming(am);
                        a.ReportSent(am.Data.Length);
                        moved = true;
                    }
                    while (b.TryDequeueOutgoing(out var bm)) {
                        a.PushIncoming(bm);
                        b.ReportSent(bm.Data.Length);
                        moved = true;
                    }
                    if (!moved)
                        await Task.Delay(10);
                }
            });
        }

        public async Task DisconnectAsync(Pid pid)
        {
            if (_participants.TryRemove(pid, out var participant))
            {
                await participant.DisconnectAsync();
                foreach (var kv in _udpMap)
                {
                    if (kv.Value.Id.Equals(pid))
                        _udpMap.TryRemove(kv.Key, out _);
                }
                ParticipantDisconnected?.Invoke(pid);
            }
        }

        public IEnumerable<Pid> ListParticipants()
            => _participants.Keys;

        public bool TryGetParticipantStats(Pid pid, out (long sentBytes, long recvBytes) stats)
        {
            if (_participants.TryGetValue(pid, out var p))
            {
                stats = p.StatsSnapshot();
                return true;
            }
            stats = default;
            return false;
        }

        public (long sentBytes, long recvBytes, long sentMessages, long recvMessages) MetricsSnapshot()
            => Metrics.Snapshot();

        public async Task ShutdownAsync()
        {
            _listenCts?.Cancel();
            _tcpListener?.Stop();
            _quicListener?.Dispose();
            _udpListener?.Dispose();

            foreach (var pid in _participants.Keys)
                await DisconnectAsync(pid);

            StopMetrics();
        }
    }
}
