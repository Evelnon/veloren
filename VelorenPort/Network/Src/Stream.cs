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
        private static readonly int[] _weights = { 8, 4, 2, 1, 1, 1, 1, 1 };
        private int _currentPrio;
        private int _remainingWeight;
        private readonly System.IO.Stream? _transport;
        private readonly Metrics? _metrics;
        private readonly Participant? _participant;
        private readonly object _bandwidthLock = new();
        private readonly Timer? _bandwidthTimer;
        private long _availableBandwidth;
        private readonly ConcurrentDictionary<ulong, InFlight> _inFlight = new();
        private readonly Timer? _resendTimer;
        private readonly SemaphoreSlim _sendWindow;
        private const int MaxInFlight = 64;
        private long _nextMid;
        private bool ReliabilityEnabled => Promises.HasFlag(Promises.GuaranteedDelivery);
        public byte Priority { get; }
        public ulong GuaranteedBandwidth { get; }
        private bool _closed;
        private TaskCompletionSource<bool>? _closeTcs;

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
            _currentPrio = 0;
            _remainingWeight = _weights[0];
            _sendWindow = new SemaphoreSlim(MaxInFlight);
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
                    await _sendWindow.WaitAsync();
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

        /// <summary>
        /// Serializa y envía una estructura de datos genérica usando los
        /// parámetros del <see cref="Stream"/>.
        /// </summary>
        public Task SendAsync<T>(T value, byte prio = 0)
        {
            var msg = Message.Serialize(value, new StreamParams(Promises, Priority, GuaranteedBandwidth));
            return SendAsync(msg, prio);
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
                        if (_inFlight.TryRemove(mid, out _))
                            _sendWindow.Release();
                        return await RecvAsync();
                    } else if (kind == 0x03) {
                        await SendRawAsync(0x03, mid, Array.Empty<byte>());
                        _closed = true;
                        _closeTcs?.TrySetResult(true);
                        return null;
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
                int prio = (_currentPrio + i) % _prioTx.Length;
                if (_prioTx[prio].TryDequeue(out msg)) {
                    _remainingWeight--;
                    if (_remainingWeight <= 0) {
                        _currentPrio = (prio + 1) % _prioTx.Length;
                        _remainingWeight = _weights[_currentPrio];
                    }
                    return true;
                }
            }
            _currentPrio = (_currentPrio + 1) % _prioTx.Length;
            _remainingWeight = _weights[_currentPrio];
            msg = default!;
            return false;
        }

        public Message? TryRecv() {
            _rx.TryDequeue(out var msg);
            return msg;
        }

        /// <summary>
        /// Recibe y deserializa un mensaje genérico utilizando los parámetros
        /// del <see cref="Stream"/>.
        /// </summary>
        public async Task<T?> RecvAsync<T>()
        {
            var msg = await RecvAsync();
            return msg == null ? default : msg.Deserialize<T>();
        }

        /// <summary>
        /// Intentar recibir y deserializar sin bloquear.
        /// </summary>
        public T? TryRecv<T>()
        {
            var msg = TryRecv();
            return msg == null ? default : msg.Deserialize<T>();
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

        public async Task CloseAsync()
        {
            if (_closed) return;
            _closed = true;
            if (_transport != null && ReliabilityEnabled)
            {
                _closeTcs = new TaskCompletionSource<bool>();
                try { await SendRawAsync(0x03, 0, Array.Empty<byte>()); } catch { /* ignore */ }
                await Task.WhenAny(_closeTcs.Task, Task.Delay(1000));
            }
        }

        public void Dispose()
        {
            if (!_closed)
                CloseAsync().GetAwaiter().GetResult();
            if (_participant != null)
                _metrics?.StreamClosed(_participant.Id);
            else
                _metrics?.StreamClosed(new Pid(Guid.Empty));
            _participant?.RemoveStream(Id);
            _transport?.Dispose();
            _bandwidthTimer?.Dispose();
            _resendTimer?.Dispose();
            _sendWindow.Dispose();
        }
    }
}
