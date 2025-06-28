using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents a unidirectional stream between two participants.
    /// </summary>
    public class Stream : IDisposable {
        private class InFlight
        {
            public Message Msg { get; }
            public ulong Mid { get; }
            public DateTime LastSent { get; set; }
            public InFlight(Message msg, ulong mid)
            {
                Msg = msg;
                Mid = mid;
                LastSent = DateTime.UtcNow;
            }
        }

        public Sid Id { get; }
        public Promises Promises { get; }
        private readonly ConcurrentQueue<Message> _rx = new();
        private readonly ConcurrentQueue<Message>[] _prioTx = new ConcurrentQueue<Message>[8];
        private readonly System.IO.Stream? _transport;
        private readonly Metrics? _metrics;
        private readonly Participant? _participant;
        private readonly object _bandwidthLock = new();
        private readonly Timer? _bandwidthTimer;
        private long _availableBandwidth;
        private readonly ConcurrentDictionary<ulong, InFlight> _inFlight = new();
        private readonly Timer? _resendTimer;
        private long _nextMid;
        private bool ReliabilityEnabled => Promises.HasFlag(Promises.GuaranteedDelivery);
        public byte Priority { get; }
        public ulong GuaranteedBandwidth { get; }

        internal Stream(
            Sid id,
            Promises promises,
            System.IO.Stream? transport = null,
            byte priority = 0,
            ulong guaranteedBandwidth = 0,
            Metrics? metrics = null,
            Participant? participant = null) {
            Id = id;
            Promises = promises;
            _transport = transport;
            Priority = priority;
            GuaranteedBandwidth = guaranteedBandwidth;
            _metrics = metrics;
            _participant = participant;
            for (int i = 0; i < _prioTx.Length; i++) _prioTx[i] = new ConcurrentQueue<Message>();
            if (GuaranteedBandwidth > 0) {
                _availableBandwidth = (long)GuaranteedBandwidth;
                _bandwidthTimer = new Timer(_ =>
                {
                    lock (_bandwidthLock)
                        _availableBandwidth = (long)GuaranteedBandwidth;
                }, null, 1000, 1000);
            }
            if (_transport != null && ReliabilityEnabled) {
                _resendTimer = new Timer(async _ => await ResendAsync(), null, 500, 500);
            }
        }

        public async Task SendAsync(Message msg, byte prio = 0) {
            if (_transport != null) {
                if (ReliabilityEnabled) {
                    ulong mid = (ulong)Interlocked.Increment(ref _nextMid);
                    _inFlight[mid] = new InFlight(msg, mid);
                    await SendRawAsync(0x01, mid, msg.Data);
                } else {
                    int total = msg.Data.Length + 4;
                    await AcquireBandwidthAsync(total);
                    var length = BitConverter.GetBytes(msg.Data.Length);
                    await _transport.WriteAsync(length, 0, length.Length);
                    await _transport.WriteAsync(msg.Data, 0, msg.Data.Length);
                    await _transport.FlushAsync();
                    _metrics?.CountSent(total);
                    _participant?.ReportSent(total);
                }
            } else {
                if (prio >= _prioTx.Length) prio = (byte)(_prioTx.Length - 1);
                _prioTx[prio].Enqueue(msg);
            }
        }

        public async Task<Message?> RecvAsync() {
            if (_transport != null) {
                var lenBuf = new byte[4];
                if (await ReadExactAsync(_transport, lenBuf, 4) == 0)
                    return null;
                int len = BitConverter.ToInt32(lenBuf, 0);
                var buf = new byte[len];
                if (await ReadExactAsync(_transport, buf, len) == 0)
                    return null;
                _metrics?.CountReceived(4 + len);
                _participant?.ReportReceived(4 + len);
                if (ReliabilityEnabled) {
                    byte kind = buf[0];
                    ulong mid = BitConverter.ToUInt64(buf, 1);
                    if (kind == 0x02) {
                        _inFlight.TryRemove(mid, out _);
                        return await RecvAsync();
                    } else {
                        var payload = new byte[len - 9];
                        Buffer.BlockCopy(buf, 9, payload, 0, payload.Length);
                        await SendRawAsync(0x02, mid, Array.Empty<byte>());
                        return new Message(payload, false);
                    }
                }
                return new Message(buf, false);
            }
            _rx.TryDequeue(out var msg);
            return msg;
        }

        private static async Task<int> ReadExactAsync(System.IO.Stream stream, byte[] buffer, int len) {
            int read = 0;
            while (read < len) {
                int r = await stream.ReadAsync(buffer, read, len - read);
                if (r == 0) return read;
                read += r;
            }
            return read;
        }

        internal void PushIncoming(Message msg)
        {
            _rx.Enqueue(msg);
            _participant?.ReportReceived(msg.Data.Length);
        }

        internal void ReportSent(int bytes) => _participant?.ReportSent(bytes);
        internal bool TryDequeueOutgoing(out Message msg) {
            for (int i = 0; i < _prioTx.Length; i++) {
                if (_prioTx[i].TryDequeue(out msg)) return true;
            }
            msg = default!;
            return false;
        }

        public Message? TryRecv() {
            _rx.TryDequeue(out var msg);
            return msg;
        }

        private async Task SendRawAsync(byte kind, ulong mid, byte[] payload) {
            if (_transport == null) return;
            int total = payload.Length + 4 + 9;
            await AcquireBandwidthAsync(total);
            var len = BitConverter.GetBytes(payload.Length + 9);
            await _transport.WriteAsync(len, 0, len.Length);
            await _transport.WriteAsync(new[] { kind }, 0, 1);
            await _transport.WriteAsync(BitConverter.GetBytes(mid), 0, 8);
            if (payload.Length > 0)
                await _transport.WriteAsync(payload, 0, payload.Length);
            await _transport.FlushAsync();
            _metrics?.CountSent(total);
            _participant?.ReportSent(total);
        }

        private async Task ResendAsync()
        {
            foreach (var kv in _inFlight)
            {
                if ((DateTime.UtcNow - kv.Value.LastSent).TotalMilliseconds > 500)
                {
                    kv.Value.LastSent = DateTime.UtcNow;
                    await SendRawAsync(0x01, kv.Key, kv.Value.Msg.Data);
                }
            }
        }

        private async Task AcquireBandwidthAsync(int bytes)
        {
            if (GuaranteedBandwidth == 0) return;
            while (true)
            {
                lock (_bandwidthLock)
                {
                    if (_availableBandwidth >= bytes)
                    {
                        _availableBandwidth -= bytes;
                        return;
                    }
                }
                await Task.Delay(5);
            }
        }

        public void Dispose()
        {
            _metrics?.StreamClosed();
            _transport?.Dispose();
            _bandwidthTimer?.Dispose();
            _resendTimer?.Dispose();
        }
    }
}
