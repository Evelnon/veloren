using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents a unidirectional stream between two participants.
    /// </summary>
    public class Stream {
        public Sid Id { get; }
        public Promises Promises { get; }
        private readonly ConcurrentQueue<Message> _rx = new();
        private readonly ConcurrentQueue<Message>[] _prioTx = new ConcurrentQueue<Message>[8];
        private readonly System.IO.Stream? _transport;
        public byte Priority { get; }
        public ulong GuaranteedBandwidth { get; }

        internal Stream(Sid id, Promises promises, System.IO.Stream? transport = null, byte priority = 0, ulong guaranteedBandwidth = 0) {
            Id = id;
            Promises = promises;
            _transport = transport;
            Priority = priority;
            GuaranteedBandwidth = guaranteedBandwidth;
            for (int i = 0; i < _prioTx.Length; i++) _prioTx[i] = new ConcurrentQueue<Message>();
        }

        public async Task SendAsync(Message msg, byte prio = 0) {
            if (_transport != null) {
                var length = BitConverter.GetBytes(msg.Data.Length);
                await _transport.WriteAsync(length, 0, length.Length);
                await _transport.WriteAsync(msg.Data, 0, msg.Data.Length);
                await _transport.FlushAsync();
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

        internal void PushIncoming(Message msg) => _rx.Enqueue(msg);
        internal bool TryDequeueOutgoing(out Message msg) {
            for (int i = 0; i < _prioTx.Length; i++) {
                if (_prioTx[i].TryDequeue(out msg)) return true;
            }
            msg = default!;
            return false;
        }
    }
}
