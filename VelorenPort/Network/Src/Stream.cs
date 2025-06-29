using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents a unidirectional stream between two participants.
    /// </summary>
    public class Stream : IDisposable {
        private class InFlight
        {
            public Message Msg { get; }
            public ulong Mid { get; }
            public byte Priority { get; }
            public DateTime LastSent { get; set; }

            public InFlight(Message msg, ulong mid, byte priority)
            {
                Msg = msg;
                Mid = mid;
                Priority = priority;
                LastSent = DateTime.UtcNow;
            }
        }

        public Sid Id { get; }
        public Promises Promises { get; }
        private readonly ConcurrentQueue<Message> _rx = new();
        private readonly ConcurrentQueue<Message>[] _prioTx = new ConcurrentQueue<Message>[8];
        private static int[] _weights = { 8, 4, 2, 1, 1, 1, 1, 1 };
        internal static int PriorityLevels => _weights.Length;
        internal static int GetWeight(int index) => _weights[index];
        private int _currentPrio;
        private int _remainingWeight;
        private readonly System.IO.Stream? _transport;
        private readonly UdpClient? _udpClient;
        private readonly IPEndPoint? _udpRemote;
        private readonly byte[]? _encKey;
        private readonly Metrics? _metrics;
        private readonly Participant? _participant;
        private readonly object _bandwidthLock = new();
        private readonly Timer? _bandwidthTimer;
        private long _availableBandwidth;
        private readonly ConcurrentDictionary<ulong, InFlight> _inFlight = new();
        private readonly Timer? _resendTimer;
        private readonly SemaphoreSlim[] _sendWindows;
        private const int DefaultWindow = 10;
        private const int MaxWindow = 64;
        private const int MinWindow = 1;
        private readonly int[] _cwnds;
        private readonly double[] _srtts;
        private readonly object[] _cwndLocks;
        private long _nextMid;
        private ulong _ackBase;
        private ulong _ackMask;
        private const byte DebugKind = 0x04;
        private bool ReliabilityEnabled => Promises.HasFlag(Promises.GuaranteedDelivery);
        private bool EncryptionEnabled => Promises.HasFlag(Promises.Encrypted);
        public byte Priority { get; }
        public ulong GuaranteedBandwidth { get; }
        private bool _closed;
        private TaskCompletionSource<bool>? _closeTcs;

        internal Participant? Owner => _participant;

        internal Stream(
            Sid id,
            Promises promises,
            System.IO.Stream? transport = null,
            byte priority = 0,
            ulong guaranteedBandwidth = 0,
            Metrics? metrics = null,
            Participant? participant = null,
            UdpClient? udpClient = null,
            IPEndPoint? udpRemote = null) {
            Id = id;
            Promises = promises;
            _transport = transport;
            _udpClient = udpClient;
            _udpRemote = udpRemote;
            Priority = priority;
            GuaranteedBandwidth = guaranteedBandwidth;
            _metrics = metrics;
            _participant = participant;
            if (EncryptionEnabled && _participant != null)
            {
                using var sha = SHA256.Create();
                _encKey = sha.ComputeHash(_participant.Secret.ToByteArray());
            }
            for (int i = 0; i < _prioTx.Length; i++) _prioTx[i] = new ConcurrentQueue<Message>();
            _currentPrio = 0;
            _remainingWeight = _weights[0];
            int levels = _prioTx.Length;
            _cwnds = new int[levels];
            _srtts = new double[levels];
            _cwndLocks = new object[levels];
            _sendWindows = new SemaphoreSlim[levels];
            for (int i = 0; i < levels; i++)
            {
                _cwnds[i] = DefaultWindow;
                _srtts[i] = 0;
                _cwndLocks[i] = new object();
                _sendWindows[i] = new SemaphoreSlim(_cwnds[i]);
            }
            if (GuaranteedBandwidth > 0) {
                _availableBandwidth = (long)GuaranteedBandwidth;
                _bandwidthTimer = new Timer(_ =>
                {
                    lock (_bandwidthLock)
                        _availableBandwidth = (long)GuaranteedBandwidth;
                }, null, 1000, 1000);
            }
            if ((_transport != null || _udpClient != null) && ReliabilityEnabled) {
                _resendTimer = new Timer(async _ => await ResendAsync(), null, 500, 500);
            }
        }

        public static void SetPriorityWeights(int[] weights)
        {
            if (weights.Length == 0) return;
            _weights = new int[weights.Length];
            for (int i = 0; i < weights.Length; i++)
                _weights[i] = Math.Max(1, weights[i]);
        }

        public async Task SendAsync(Message msg, byte prio = 0) {
#if DEBUG
            msg.Verify(new StreamParams(Promises, Priority, GuaranteedBandwidth));
#endif
            if (_transport != null) {
                if (ReliabilityEnabled) {
                    if (prio >= _sendWindows.Length) prio = (byte)(_sendWindows.Length - 1);
                    await _sendWindows[prio].WaitAsync();
                    ulong mid = (ulong)Interlocked.Increment(ref _nextMid);
                    _inFlight[mid] = new InFlight(msg, mid, prio);
                    _metrics?.RetransmitQueueSize(_participant?.Id ?? new Pid(Guid.Empty), Id, _inFlight.Count);
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
            } else if (_udpClient != null) {
                if (ReliabilityEnabled) {
                    if (prio >= _sendWindows.Length) prio = (byte)(_sendWindows.Length - 1);
                    await _sendWindows[prio].WaitAsync();
                    ulong mid = (ulong)Interlocked.Increment(ref _nextMid);
                    _inFlight[mid] = new InFlight(msg, mid, prio);
                    _metrics?.RetransmitQueueSize(_participant?.Id ?? new Pid(Guid.Empty), Id, _inFlight.Count);
                    await SendRawAsync(0x01, mid, msg.Data);
                } else {
                    await SendRawAsync(0x00, 0, msg.Data);
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
                        HandleAck(mid, buf.Length > 9 ? buf.AsSpan(9).ToArray() : Array.Empty<byte>());
                        return await RecvAsync();
                    } else if (kind == 0x03) {
                        await SendRawAsync(0x03, mid, Array.Empty<byte>());
                        _closed = true;
                        _closeTcs?.TrySetResult(true);
                        return null;
                    } else {
                        var payload = new byte[len - 9];
                        Buffer.BlockCopy(buf, 9, payload, 0, payload.Length);
                        if (EncryptionEnabled)
                            payload = Decrypt(payload);
                        UpdateAckState(mid);
                        await SendAckAsync();
                        var message = new Message(payload, Promises.HasFlag(Promises.Compressed));
#if DEBUG
                        message.Verify(new StreamParams(Promises, Priority, GuaranteedBandwidth));
#endif
                        return message;
                    }
                }
                var data = EncryptionEnabled ? Decrypt(buf) : buf;
                var m = new Message(data, Promises.HasFlag(Promises.Compressed));
#if DEBUG
                m.Verify(new StreamParams(Promises, Priority, GuaranteedBandwidth));
#endif
                return m;
            }
            _rx.TryDequeue(out var msg);
#if DEBUG
            msg?.Verify(new StreamParams(Promises, Priority, GuaranteedBandwidth));
#endif
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

        internal void ProcessDatagram(byte kind, ulong mid, byte[] payload)
        {
            if (kind == 0x02)
            {
                HandleAck(mid, payload);
                return;
            }
            if (kind == 0x03)
            {
                _ = SendRawAsync(0x03, mid, Array.Empty<byte>());
                _closed = true;
                _closeTcs?.TrySetResult(true);
                return;
            }
            if (kind == DebugKind)
            {
                var msg = System.Text.Encoding.UTF8.GetString(payload);
                _participant?.HandleDebugFrame(msg);
                return;
            }

            if (ReliabilityEnabled)
            {
                UpdateAckState(mid);
                _ = SendAckAsync();
            }
            if (EncryptionEnabled)
                payload = Decrypt(payload);
            var message = new Message(payload, Promises.HasFlag(Promises.Compressed));
#if DEBUG
            message.Verify(new StreamParams(Promises, Priority, GuaranteedBandwidth));
#endif
            PushIncoming(message);
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
            var data = (EncryptionEnabled && kind == 0x01 && payload.Length > 0) ? Encrypt(payload) : payload;
            if (_transport != null) {
                int total = data.Length + 4 + 9;
                await AcquireBandwidthAsync(total);
                var len = BitConverter.GetBytes(data.Length + 9);
                await _transport.WriteAsync(len, 0, len.Length);
                await _transport.WriteAsync(new[] { kind }, 0, 1);
                await _transport.WriteAsync(BitConverter.GetBytes(mid), 0, 8);
                if (data.Length > 0)
                    await _transport.WriteAsync(data, 0, data.Length);
                await _transport.FlushAsync();
                _metrics?.CountSent(total);
                _participant?.ReportSent(total);
            } else if (_udpClient != null && _udpRemote != null) {
                const int header = 17;
                int total = data.Length + header;
                await AcquireBandwidthAsync(total);
                var buffer = new byte[total];
                BitConverter.GetBytes(Id.Value).CopyTo(buffer, 0);
                buffer[8] = kind;
                BitConverter.GetBytes(mid).CopyTo(buffer, 9);
                if (data.Length > 0)
                    Buffer.BlockCopy(data, 0, buffer, 17, data.Length);
                await _udpClient.SendAsync(buffer, buffer.Length, _udpRemote);
                _metrics?.CountSent(total);
                _participant?.ReportSent(total);
            }
        }

        internal Task SendDebugAsync(string message)
            => SendRawAsync(DebugKind, 0, System.Text.Encoding.UTF8.GetBytes(message));

        private async Task ResendAsync()
        {
            foreach (var kv in _inFlight)
            {
                if ((DateTime.UtcNow - kv.Value.LastSent).TotalMilliseconds > 500)
                {
                    kv.Value.LastSent = DateTime.UtcNow;
                    await SendRawAsync(0x01, kv.Key, kv.Value.Msg.Data);
                    var pid = _participant?.Id ?? new Pid(Guid.Empty);
                    _metrics?.FrameRetransmitted(pid, Id);
                    OnLoss(kv.Value.Priority);
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

        private byte[] Encrypt(byte[] data)
        {
            if (_encKey == null) return data;
            using var aes = Aes.Create();
            aes.Key = _encKey;
            aes.IV = new byte[16];
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }

        private byte[] Decrypt(byte[] data)
        {
            if (_encKey == null) return data;
            using var aes = Aes.Create();
            aes.Key = _encKey;
            aes.IV = new byte[16];
            using var ms = new MemoryStream(data);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var outMs = new MemoryStream();
            cs.CopyTo(outMs);
            return outMs.ToArray();
        }

        private void HandleAck(ulong mid, byte[] payload)
        {
            AckMessage(mid);
            if (payload.Length >= 8)
            {
                ulong mask = BitConverter.ToUInt64(payload, 0);
                for (int i = 0; i < 64; i++)
                {
                    if ((mask & (1UL << i)) != 0)
                        AckMessage(mid + (ulong)i + 1);
                }
            }
        }

        private void AckMessage(ulong mid)
        {
            if (_inFlight.TryRemove(mid, out var infl))
            {
                _sendWindows[infl.Priority].Release();
                var rtt = (DateTime.UtcNow - infl.LastSent).TotalMilliseconds;
                var pid = _participant?.Id ?? new Pid(Guid.Empty);
                _metrics?.StreamRtt(pid, Id, rtt);
                _metrics?.RetransmitQueueSize(pid, Id, _inFlight.Count);
                OnAck(infl.Priority, rtt);
            }
        }

        private void UpdateAckState(ulong mid)
        {
            if (mid == _ackBase + 1)
            {
                _ackBase = mid;
                while ((_ackMask & 1) != 0)
                {
                    _ackBase++;
                    _ackMask >>= 1;
                }
            }
            else if (mid > _ackBase + 1 && mid - _ackBase - 2 < 64)
            {
                int bit = (int)(mid - _ackBase - 2);
                _ackMask |= 1UL << bit;
            }
        }

        private Task SendAckAsync()
            => SendRawAsync(0x02, _ackBase, BitConverter.GetBytes(_ackMask));

        private void OnAck(byte prio, double rtt)
        {
            if (rtt > 1000) return;
            lock (_cwndLocks[prio])
            {
                _srtts[prio] = _srtts[prio] == 0 ? rtt : (_srtts[prio] * 7 + rtt) / 8.0;
                if (rtt <= _srtts[prio] && _cwnds[prio] < MaxWindow)
                {
                    _cwnds[prio]++;
                    _sendWindows[prio].Release();
                }
                else if (rtt > _srtts[prio] * 2 && _cwnds[prio] > MinWindow)
                {
                    _cwnds[prio]--;
                    _sendWindows[prio].Wait(0);
                }
            }
        }

        private void OnLoss(byte prio)
        {
            lock (_cwndLocks[prio])
            {
                int newWin = Math.Max(MinWindow, _cwnds[prio] / 2);
                int diff = _cwnds[prio] - newWin;
                _cwnds[prio] = newWin;
                for (int i = 0; i < diff; i++)
                    _sendWindows[prio].Wait(0);
            }
            var pid = _participant?.Id ?? new Pid(Guid.Empty);
            _metrics?.StreamLoss(pid, Id);
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
            {
                _metrics?.StreamClosed(_participant.Id);
                _metrics?.StreamRttReset(_participant.Id, Id);
                _metrics?.RetransmitQueueSize(_participant.Id, Id, 0);
            }
            else
            {
                _metrics?.StreamClosed(new Pid(Guid.Empty));
                _metrics?.StreamRttReset(new Pid(Guid.Empty), Id);
                _metrics?.RetransmitQueueSize(new Pid(Guid.Empty), Id, 0);
            }
            _participant?.RemoveStream(Id);
            _transport?.Dispose();
            _bandwidthTimer?.Dispose();
            _resendTimer?.Dispose();
            foreach (var sw in _sendWindows)
                sw.Dispose();
        }
    }
}
