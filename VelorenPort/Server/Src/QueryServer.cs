using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VelorenPort.Server.Sys;

namespace VelorenPort.Server {
    /// <summary>
    /// Simplified UDP based discovery server similar to the Rust query server.
    /// It listens for datagrams on a configured port and responds with either a
    /// challenge value or basic server information.
    /// </summary>
    public class QueryServer {
        const ushort VERSION = 0;
        static readonly byte[] HEADER = { (byte)'v', (byte)'e', (byte)'l', (byte)'o', (byte)'r', (byte)'e', (byte)'n' };
        static readonly TimeSpan SECRET_REGEN_INTERVAL = TimeSpan.FromMinutes(5);

        readonly IPEndPoint _endpoint;
        readonly UdpClient _socket;
        readonly RateLimiter _rateLimiter;
        readonly QueryServerMetrics _metrics = new();
        (ulong, ulong) _secrets;
        DateTime _lastSecretRefresh = DateTime.UtcNow;
        ServerInfo _info;
        readonly object _lock = new();

        public QueryServerMetrics Metrics => _metrics;

        public QueryServer(IPEndPoint endpoint, ServerInfo info, ushort ratelimit = 120) {
            _endpoint = endpoint;
            _socket = new UdpClient(endpoint);
            _info = info;
            _rateLimiter = new RateLimiter(ratelimit);
            RegenerateSecrets();
        }

        public void UpdateInfo(ServerInfo info) {
            lock (_lock) {
                _info = info;
            }
        }

        public async Task RunAsync(CancellationToken token) {
            while (!token.IsCancellationRequested) {
                try {
                    var result = await _socket.ReceiveAsync(token);
                    ProcessDatagram(result.Buffer, result.RemoteEndPoint);
                    if (DateTime.UtcNow - _lastSecretRefresh > SECRET_REGEN_INTERVAL)
                        RegenerateSecrets();
                } catch (OperationCanceledException) {
                    break;
                } catch { }
            }
        }

        void RegenerateSecrets() {
            Span<byte> buf = stackalloc byte[16];
            RandomNumberGenerator.Fill(buf);
            _secrets = (BitConverter.ToUInt64(buf[..8]), BitConverter.ToUInt64(buf[8..]));
            _lastSecretRefresh = DateTime.UtcNow;
        }

        void ProcessDatagram(byte[] data, IPEndPoint remote) {
            _metrics.ReceivedPackets.Inc();
            if (!ValidateDatagram(data)) {
                _metrics.DroppedPackets.Inc();
                return;
            }

            ulong p = BitConverter.ToUInt64(data, 2);
            var request = (QueryServerRequest)data[10];
            ulong realP = ComputeChallenge(remote.Address);

            if (p != realP) {
                _metrics.InitRequests.Inc();
                SendInitResponse(realP, remote);
                return;
            }

            if (!_rateLimiter.CanRequest(remote.Address)) {
                _metrics.Ratelimited.Inc();
                return;
            }

            switch (request) {
                case QueryServerRequest.Init:
                    _metrics.InitRequests.Inc();
                    SendInitResponse(realP, remote);
                    break;
                case QueryServerRequest.ServerInfo:
                    _metrics.InfoRequests.Inc();
                    ServerInfo info;
                    lock (_lock) info = _info;
                    SendInfoResponse(info, remote);
                    break;
                default:
                    _metrics.InvalidPackets.Inc();
                    break;
            }
        }

        static bool ValidateDatagram(byte[] data) {
            if (data.Length < HEADER.Length + 11)
                return false;
            for (int i = 0; i < HEADER.Length; i++)
                if (data[^HEADER.Length + i] != HEADER[i])
                    return false;
            ushort version = BitConverter.ToUInt16(data, 0);
            return version == VERSION;
        }

        void SendInitResponse(ulong p, IPEndPoint addr) {
            var bytes = QueryProtocol.SerializeInitResponse(p);
            _ = _socket.SendAsync(bytes, bytes.Length, addr);
            _metrics.SentResponses.Inc();
        }

        void SendInfoResponse(ServerInfo info, IPEndPoint addr) {
            var bytes = QueryProtocol.SerializeInfoResponse(info);
            _ = _socket.SendAsync(bytes, bytes.Length, addr);
            _metrics.SentResponses.Inc();
        }

        ulong ComputeChallenge(IPAddress ip) {
            ulong hash = _secrets.Item1;
            foreach (byte b in ip.GetAddressBytes())
                hash = (hash * 0x100000001b3) ^ b;
            return hash ^ _secrets.Item2;
        }
    }

    public enum QueryServerRequest : byte {
        Init = 0,
        ServerInfo = 1,
    }

    static class QueryProtocol {
        public const ushort VERSION = 0;

        static readonly byte[] HEADER = { (byte)'v', (byte)'e', (byte)'l', (byte)'o', (byte)'r', (byte)'e', (byte)'n' };

        public static byte[] SerializeRequest(ulong p, QueryServerRequest req) {
            var bytes = new byte[2 + 8 + 1 + HEADER.Length];
            BitConverter.TryWriteBytes(bytes.AsSpan(0, 2), VERSION);
            BitConverter.TryWriteBytes(bytes.AsSpan(2, 8), p);
            bytes[10] = (byte)req;
            HEADER.CopyTo(bytes, bytes.Length - HEADER.Length);
            return bytes;
        }

        public static byte[] SerializeInitResponse(ulong p) {
            var bytes = new byte[2 + 1 + 8 + 2 + HEADER.Length];
            BitConverter.TryWriteBytes(bytes.AsSpan(0, 2), VERSION);
            bytes[2] = 1; // variant Init
            BitConverter.TryWriteBytes(bytes.AsSpan(3, 8), p);
            BitConverter.TryWriteBytes(bytes.AsSpan(11, 2), VERSION);
            HEADER.CopyTo(bytes, bytes.Length - HEADER.Length);
            return bytes;
        }

        public static byte[] SerializeInfoResponse(ServerInfo info) {
            var bytes = new byte[2 + 1 + 1 + 4 + 8 + 2 + 2 + 1 + HEADER.Length];
            BitConverter.TryWriteBytes(bytes.AsSpan(0, 2), VERSION);
            bytes[2] = 0; // variant Response
            bytes[3] = 0; // subvariant ServerInfo
            BitConverter.TryWriteBytes(bytes.AsSpan(4, 4), info.GitHash);
            BitConverter.TryWriteBytes(bytes.AsSpan(8, 8), info.GitTimestamp);
            BitConverter.TryWriteBytes(bytes.AsSpan(16, 2), info.PlayersCount);
            BitConverter.TryWriteBytes(bytes.AsSpan(18, 2), info.PlayerCap);
            bytes[20] = (byte)info.BattleMode;
            HEADER.CopyTo(bytes, bytes.Length - HEADER.Length);
            return bytes;
        }

        public static bool TryParseInitResponse(ReadOnlySpan<byte> data, out ulong p) {
            p = 0;
            if (data.Length < 2 + 1 + 8 + 2 + HEADER.Length) return false;
            if (BitConverter.ToUInt16(data) != VERSION) return false;
            if (data[2] != 1) return false;
            p = BitConverter.ToUInt64(data.Slice(3));
            return true;
        }

        public static bool TryParseInfoResponse(ReadOnlySpan<byte> data, out ServerInfo info) {
            info = default;
            if (data.Length < 2 + 1 + 1 + 4 + 8 + 2 + 2 + 1 + HEADER.Length) return false;
            if (BitConverter.ToUInt16(data) != VERSION) return false;
            if (data[2] != 0 || data[3] != 0) return false;
            info = new ServerInfo(
                BitConverter.ToUInt32(data.Slice(4)),
                BitConverter.ToInt64(data.Slice(8)),
                BitConverter.ToUInt16(data.Slice(16)),
                BitConverter.ToUInt16(data.Slice(18)),
                (BattleMode)data[20]);
            return true;
        }
    }

    class RateLimiter {
        readonly int _limit;
        readonly TimeSpan WINDOW = TimeSpan.FromMinutes(1);
        readonly object _lock = new();
        readonly System.Collections.Generic.Dictionary<IPAddress, System.Collections.Generic.List<DateTime>> _states = new();

        public RateLimiter(int limit) { _limit = limit; }

        public bool CanRequest(IPAddress ip) {
            lock (_lock) {
                if (!_states.TryGetValue(ip, out var list)) {
                    list = new System.Collections.Generic.List<DateTime>();
                    _states[ip] = list;
                }
                var now = DateTime.UtcNow;
                list.RemoveAll(t => (now - t) > WINDOW);
                if (list.Count >= _limit)
                    return false;
                list.Add(now);
                return true;
            }
        }
    }
}
