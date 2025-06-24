using System;
using System.Threading;

namespace VelorenPort.Network {
    /// <summary>
    /// Simple network metrics collector similar to the Rust crate.
    /// Tracks sent and received bytes and message counts.
    /// </summary>
    public class Metrics {
        private long _sentBytes;
        private long _recvBytes;
        private long _sentMessages;
        private long _recvMessages;

        public void CountSent(int bytes) {
            Interlocked.Add(ref _sentBytes, bytes);
            Interlocked.Increment(ref _sentMessages);
        }

        public void CountReceived(int bytes) {
            Interlocked.Add(ref _recvBytes, bytes);
            Interlocked.Increment(ref _recvMessages);
        }

        public (long sentBytes, long recvBytes, long sentMessages, long recvMessages) Snapshot()
            => (Interlocked.Read(ref _sentBytes), Interlocked.Read(ref _recvBytes),
                Interlocked.Read(ref _sentMessages), Interlocked.Read(ref _recvMessages));
    }
}
