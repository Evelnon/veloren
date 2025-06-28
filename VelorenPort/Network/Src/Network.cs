using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Net.Quic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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
        private TcpListener? _tcpListener;
        private QuicListener? _quicListener;
        private CancellationTokenSource? _listenCts;

        public Network(Pid pid) {
            LocalPid = pid;
        }

        public Task ListenAsync(ListenAddr addr) {
            _listenCts?.Cancel();
            _listenCts = new CancellationTokenSource();

            switch (addr) {
                case ListenAddr.Tcp tcp:
                    _tcpListener = new TcpListener(tcp.EndPoint);
                    _tcpListener.Start();
                    _ = AcceptTcpAsync(_listenCts.Token);
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
                    var p = new Participant(Pid.NewPid(), addr, client);
                    _participants[p.Id] = p;
                    _pending.Enqueue(p);
                    return p;
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
                    var qp = new Participant(Pid.NewPid(), addr, null, conn);
                    _participants[qp.Id] = qp;
                    _pending.Enqueue(qp);
                    return qp;
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
                var participant = new Participant(Pid.NewPid(), new ConnectAddr.Tcp(ep), client);
                _participants[participant.Id] = participant;
                _pending.Enqueue(participant);
            }
        }

        private async Task AcceptQuicAsync(CancellationToken token) {
            if (_quicListener == null) return;
            while (!token.IsCancellationRequested) {
                var connection = await _quicListener.AcceptConnectionAsync(token);
                var ep = connection.RemoteEndPoint as IPEndPoint ?? new IPEndPoint(IPAddress.Any, 0);
                var participant = new Participant(Pid.NewPid(), new ConnectAddr.Quic(ep, new QuicClientConfig(), "quic"), null, connection);
                _participants[participant.Id] = participant;
                _pending.Enqueue(participant);
            }
        }
    }
}
