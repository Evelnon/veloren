using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using VelorenPort.Server.Sys;

namespace VelorenPort.Server {
    public enum QueryClientError {
        Io,
        InvalidResponse,
        Timeout,
        ChallengeFailed
    }

    class QueryClient {
        readonly IPEndPoint _addr;
        ClientInitData? _init;
        const int MAX_REQUEST_RETRIES = 5;
        const int TIMEOUT_SECONDS = 2;

        record ClientInitData(ulong P, ushort ServerMaxVersion);

        public QueryClient(IPEndPoint addr) {
            _addr = addr;
        }

        public async Task<(ServerInfo Info, TimeSpan Ping)> ServerInfoAsync() {
            var (info, ping) = await SendQueryAsync(QueryServerRequest.ServerInfo);
            return (info, ping);
        }

        async Task<(ServerInfo, TimeSpan)> SendQueryAsync(QueryServerRequest request) {
            using var socket = new UdpClient(_addr.AddressFamily);
            if (_addr.AddressFamily == AddressFamily.InterNetwork)
                socket.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            else
                socket.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));

            for (int i = 0; i < MAX_REQUEST_RETRIES; i++) {
                byte[] req;
                if (_init != null) {
                    req = QueryProtocol.SerializeRequest(_init.P, request);
                } else {
                    req = QueryProtocol.SerializeRequest(0, QueryServerRequest.Init);
                }

                var sentAt = DateTime.UtcNow;
                try {
                    await socket.SendAsync(req, req.Length, _addr);
                } catch (Exception) {
                    throw new QueryClientException(QueryClientError.Io);
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SECONDS));
                Task<UdpReceiveResult> recvTask = socket.ReceiveAsync(cts.Token);
                try {
                    var res = await recvTask;
                    var data = res.Buffer;
                    if (QueryProtocol.TryParseInfoResponse(data, out var info)) {
                        return (info, DateTime.UtcNow - sentAt);
                    }
                    if (QueryProtocol.TryParseInitResponse(data, out var p)) {
                        _init = new ClientInitData(p, QueryProtocol.VERSION);
                        continue;
                    }
                    throw new QueryClientException(QueryClientError.InvalidResponse);
                } catch (OperationCanceledException) {
                    throw new QueryClientException(QueryClientError.Timeout);
                }
            }
            throw new QueryClientException(QueryClientError.ChallengeFailed);
        }
    }

    class QueryClientException : Exception {
        public QueryClientError Error { get; }
        public QueryClientException(QueryClientError error) {
            Error = error;
        }
    }
}
