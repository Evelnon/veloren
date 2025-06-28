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
    /// Manejador de red para Unity. Mantiene una lista de participantes
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
        public Metrics Metrics { get; } = new();

        public Network(Pid pid) {
            LocalPid = pid;
        }

        public void StartMetrics(int port = 9091) => Metrics.StartPrometheus(port);
        public void StopMetrics() => Metrics.StopPrometheus();

        public Task ListenAsync(ListenAddr addr) {
            _listenCts?.Cancel();
            _listenCts = new CancellationTokenSource();

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

        public async Task<Participant> ConnectAsync(ConnectAddr addr) {
            switch (addr) {
                case ConnectAddr.Tcp tcp:
                    var client = new TcpClient();
                    await client.ConnectAsync(tcp.EndPoint.Address, tcp.EndPoint.Port);
                    var (rpid, rsec) = await Handshake.PerformAsync(client.GetStream(), true, LocalPid, _localSecret);
                    var p = new Participant(rpid, addr, rsec, client, null, null, Metrics);
                    _participants[p.Id] = p;
                    _pending.Enqueue(p);
                    return p;
                case ConnectAddr.Udp udp:
                    var u = new UdpClient();
                    await u.ConnectAsync(udp.EndPoint);
                    var (upid, usecret) = await SendUdpHandshakeAsync(u, udp.EndPoint, LocalPid, _localSecret);
                    var up = new Participant(upid, addr, usecret, null, null, u, Metrics);
                    _participants[up.Id] = up;
                    _pending.Enqueue(up);
                    return up;
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
                    var (qpid, qsec) = await Handshake.PerformAsync(hs, true, LocalPid, _localSecret);
                    await hs.DisposeAsync();
                    var qp = new Participant(qpid, addr, qsec, null, conn, null, Metrics);
                    _participants[qp.Id] = qp;
                    _pending.Enqueue(qp);
                    return qp;
                case ConnectAddr.Mpsc mpsc:
                    if (!_mpscListeners.TryGetValue(mpsc.ChannelId, out var remote))
                        throw new NetworkConnectError.Io(new InvalidOperationException("no listener"));
                    var secret = Guid.NewGuid();
                    var remoteParticipant = new Participant(LocalPid, new ConnectAddr.Mpsc(mpsc.ChannelId), secret, null, null, null, remote.Metrics);
                    var localParticipant = new Participant(remote.LocalPid, addr, secret, null, null, null, Metrics);
                    var ls = await localParticipant.OpenStreamAsync(new Sid(1), new StreamParams(Promises.Ordered));
                    var rs = await remoteParticipant.OpenStreamAsync(new Sid(1), new StreamParams(Promises.Ordered));
                    StartMpscRelay(ls, rs);
                    _participants[localParticipant.Id] = localParticipant;
                    remote._participants[remoteParticipant.Id] = remoteParticipant;
                    _pending.Enqueue(localParticipant);
                    remote._pending.Enqueue(remoteParticipant);
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
                var (pid, secret) = await Handshake.PerformAsync(client.GetStream(), false, LocalPid, _localSecret, token);
                var participant = new Participant(pid, new ConnectAddr.Tcp(ep), secret, client, null, null, Metrics);
                _participants[participant.Id] = participant;
                _pending.Enqueue(participant);
            }
        }

        private async Task AcceptUdpAsync(CancellationToken token) {
            if (_udpListener == null) return;
            while (!token.IsCancellationRequested) {
                var result = await _udpListener.ReceiveAsync(token);
                var remote = result.RemoteEndPoint;
                if (!_udpMap.TryGetValue(remote, out var participant)) {
                    if (Handshake.TryParse(result.Buffer, out var pid, out var secret)) {
                        var reply = Handshake.GetBytes(LocalPid, _localSecret);
                        _udpListener.SendAsync(reply, reply.Length, remote).ConfigureAwait(false);
                        participant = new Participant(pid, new ConnectAddr.Udp(remote), secret, null, null, _udpListener, Metrics);
                        _participants[participant.Id] = participant;
                        _udpMap[remote] = participant;
                        _pending.Enqueue(participant);
                        await participant.OpenStreamAsync(new Sid(1), new StreamParams(Promises.Ordered));
                    }
                } else {
                    // deliver datagram to participant's streams (simplified)
                    var msg = new Message(result.Buffer, false);
                    foreach (var s in participant.IncomingStreams())
                        s.PushIncoming(msg);
                }
            }
        }

        private async Task AcceptQuicAsync(CancellationToken token) {
            if (_quicListener == null) return;
            while (!token.IsCancellationRequested) {
                var connection = await _quicListener.AcceptConnectionAsync(token);
                var ep = connection.RemoteEndPoint as IPEndPoint ?? new IPEndPoint(IPAddress.Any, 0);
                var hs = await connection.AcceptInboundStreamAsync(token);
                var (pid, secret) = await Handshake.PerformAsync(hs, false, LocalPid, _localSecret, token);
                await hs.DisposeAsync();
                var participant = new Participant(pid, new ConnectAddr.Quic(ep, new QuicClientConfig(), "quic"), secret, null, connection, null, Metrics);
                _participants[participant.Id] = participant;
                _pending.Enqueue(participant);
            }
        }

        private static bool IsHandshake(byte[] data) {
            if (data.Length != Handshake.MagicNumber.Length + Handshake.NetworkVersion.Length * 4)
                return false;
            for (int i = 0; i < Handshake.MagicNumber.Length; i++)
                if (data[i] != Handshake.MagicNumber[i])
                    return false;
            return true;
        }

        private static async Task<(Pid pid, Guid secret)> SendUdpHandshakeAsync(UdpClient client, IPEndPoint remote, Pid localPid, Guid localSecret) {
            var buffer = Handshake.GetBytes(localPid, localSecret);
            await client.SendAsync(buffer, buffer.Length);
            var result = await client.ReceiveAsync();
            if (!Handshake.TryParse(result.Buffer, out var pid, out var secret))
                throw new NetworkConnectError.Handshake(new InitProtocolError<ProtocolsError>.NotHandshake());
            return (pid, secret);
        }

        private static void StartMpscRelay(Stream a, Stream b) {
            _ = Task.Run(async () => {
                while (true) {
                    var moved = false;
                    while (a.TryDequeueOutgoing(out var am)) {
                        b.PushIncoming(am);
                        moved = true;
                    }
                    while (b.TryDequeueOutgoing(out var bm)) {
                        a.PushIncoming(bm);
                        moved = true;
                    }
                    if (!moved)
                        await Task.Delay(10);
                }
            });
        }

        public Task DisconnectAsync(Pid pid)
        {
            if (_participants.TryRemove(pid, out var participant))
            {
                participant.Close();
                foreach (var kv in _udpMap)
                {
                    if (kv.Value.Id.Equals(pid))
                        _udpMap.TryRemove(kv.Key, out _);
                }
            }
            return Task.CompletedTask;
        }

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
