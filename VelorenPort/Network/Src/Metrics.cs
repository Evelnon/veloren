using System;
using System.Threading;
using Prometheus;

namespace VelorenPort.Network {
    /// <summary>
    /// Simple network metrics collector similar to the Rust crate.
    /// Tracks sent and received bytes and message counts.
    /// </summary>
    public class Metrics {
        private readonly Counter _sentBytesCounter = MetricsCreator.CreateCounter("network_sent_bytes", "Total bytes sent");
        private readonly Counter _recvBytesCounter = MetricsCreator.CreateCounter("network_recv_bytes", "Total bytes received");
        private readonly Counter _sentMessagesCounter = MetricsCreator.CreateCounter("network_sent_messages", "Total messages sent");
        private readonly Counter _recvMessagesCounter = MetricsCreator.CreateCounter("network_recv_messages", "Total messages received");

        private long _sentBytes;
        private long _recvBytes;
        private long _sentMessages;
        private long _recvMessages;

        public void CountSent(int bytes) {
            Interlocked.Add(ref _sentBytes, bytes);
            Interlocked.Increment(ref _sentMessages);
            _sentBytesCounter.Inc(bytes);
            _sentMessagesCounter.Inc();
        }

        public void CountReceived(int bytes) {
            Interlocked.Add(ref _recvBytes, bytes);
            Interlocked.Increment(ref _recvMessages);
            _recvBytesCounter.Inc(bytes);
            _recvMessagesCounter.Inc();
        }

        public (long sentBytes, long recvBytes, long sentMessages, long recvMessages) Snapshot()
            => (Interlocked.Read(ref _sentBytes), Interlocked.Read(ref _recvBytes),
                Interlocked.Read(ref _sentMessages), Interlocked.Read(ref _recvMessages));

        private MetricServer? _metricServer;

        public void StartPrometheus(int port = 9091) {
            if (_metricServer != null) return;
            _metricServer = new MetricServer(port: port);
            _metricServer.Start();
        }

        public void StopPrometheus() => _metricServer?.StopAsync().GetAwaiter().GetResult();
    }
}
