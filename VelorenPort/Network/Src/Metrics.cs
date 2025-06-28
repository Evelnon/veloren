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

        private readonly Counter _participantsConnectedCounter = MetricsCreator.CreateCounter("network_participants_connected", "Participants connected");
        private readonly Counter _participantsDisconnectedCounter = MetricsCreator.CreateCounter("network_participants_disconnected", "Participants disconnected");
        private readonly Counter _streamsOpenedCounter = MetricsCreator.CreateCounter("network_streams_opened", "Streams opened");
        private readonly Counter _streamsClosedCounter = MetricsCreator.CreateCounter("network_streams_closed", "Streams closed");

        private readonly Counter _listenRequests = MetricsCreator.CreateCounter("network_listen_requests_total", "Listen requests", "protocol");
        private readonly Counter _connectRequests = MetricsCreator.CreateCounter("network_connect_requests_total", "Connect requests", "protocol");
        private readonly Counter _incomingConnections = MetricsCreator.CreateCounter("network_incoming_connections_total", "Incoming connections", "protocol");
        private readonly Counter _failedHandshakes = MetricsCreator.CreateCounter("network_failed_handshakes_total", "Failed handshakes");
        private readonly Gauge _channelsConnected = MetricsCreator.CreateGauge("network_channels_connected", "Channels currently connected", "participant");
        private readonly Gauge _participantBandwidth = MetricsCreator.CreateGauge("network_participant_bandwidth", "Bandwidth per participant", "participant");
        private readonly Gauge _networkInfo;

        private readonly string _localPid;

        public Metrics(Pid localPid)
        {
            _localPid = localPid.ToString();
            _networkInfo = MetricsCreator.CreateGauge("network_info", "Static network information", "version", "local_pid");
            var version = $"{Protocol.Types.VELOREN_NETWORK_VERSION[0]}.{Protocol.Types.VELOREN_NETWORK_VERSION[1]}.{Protocol.Types.VELOREN_NETWORK_VERSION[2]}";
            _networkInfo.WithLabels(version, _localPid).Set(1);
        }

        private long _sentBytes;
        private long _recvBytes;
        private long _sentMessages;
        private long _recvMessages;

        public void ParticipantConnected(Pid pid)
        {
            _participantsConnectedCounter.Inc();
            ChannelConnected(pid);
        }

        public void ParticipantDisconnected(Pid pid)
        {
            _participantsDisconnectedCounter.Inc();
            ChannelDisconnected(pid);
        }

        public void StreamOpened()
        {
            _streamsOpenedCounter.Inc();
        }

        public void StreamClosed()
        {
            _streamsClosedCounter.Inc();
        }

        public void ListenRequest(ListenAddr addr)
        {
            _listenRequests.WithLabels(ProtocolName(addr)).Inc();
        }

        public void ConnectRequest(ConnectAddr addr)
        {
            _connectRequests.WithLabels(ProtocolName(addr)).Inc();
        }

        public void IncomingConnection(ConnectAddr addr)
        {
            _incomingConnections.WithLabels(ProtocolName(addr)).Inc();
        }

        public void FailedHandshake() => _failedHandshakes.Inc();

        public void ChannelConnected(Pid pid)
        {
            _channelsConnected.WithLabels(pid.ToString()).Inc();
        }

        public void ChannelDisconnected(Pid pid)
        {
            _channelsConnected.WithLabels(pid.ToString()).Dec();
        }

        public void ParticipantBandwidth(Pid pid, float value)
        {
            _participantBandwidth.WithLabels(pid.ToString()).Set(value);
        }

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

        private static string ProtocolName(ListenAddr addr) => addr switch
        {
            ListenAddr.Tcp _ => "tcp",
            ListenAddr.Udp _ => "udp",
            ListenAddr.Quic _ => "quic",
            ListenAddr.Mpsc _ => "mpsc",
            _ => "unknown"
        };

        private static string ProtocolName(ConnectAddr addr) => addr switch
        {
            ConnectAddr.Tcp _ => "tcp",
            ConnectAddr.Udp _ => "udp",
            ConnectAddr.Quic _ => "quic",
            ConnectAddr.Mpsc _ => "mpsc",
            _ => "unknown"
        };

        public void StartPrometheus(int port = 9091) {
            if (_metricServer != null) return;
            _metricServer = new MetricServer(port: port);
            _metricServer.Start();
        }

        public void StopPrometheus() => _metricServer?.StopAsync().GetAwaiter().GetResult();
    }
}
