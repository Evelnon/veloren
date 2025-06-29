using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Prometheus;

namespace VelorenPort.Network
{
    /// <summary>
    /// Simple network metrics collector similar to the Rust crate.
    /// Tracks sent and received bytes and message counts.
    /// </summary>
    public class Metrics
    {
        private readonly Counter _sentBytesCounter = MetricsCreator.CreateCounter("network_sent_bytes", "Total bytes sent");
        private readonly Counter _recvBytesCounter = MetricsCreator.CreateCounter("network_recv_bytes", "Total bytes received");
        private readonly Counter _sentMessagesCounter = MetricsCreator.CreateCounter("network_sent_messages", "Total messages sent");
        private readonly Counter _recvMessagesCounter = MetricsCreator.CreateCounter("network_recv_messages", "Total messages received");

        private readonly Counter _participantsConnectedCounter = MetricsCreator.CreateCounter("network_participants_connected", "Participants connected");
        private readonly Counter _participantsDisconnectedCounter = MetricsCreator.CreateCounter("network_participants_disconnected", "Participants disconnected");
        private readonly Gauge _participantsActive = MetricsCreator.CreateGauge(
            "network_participants_active",
            "Participants currently active");
        private readonly Counter _streamsOpenedCounter =
            MetricsCreator.CreateCounter(
                "network_streams_opened",
                "Streams opened",
                "participant");
        private readonly Counter _streamsClosedCounter =
            MetricsCreator.CreateCounter(
                "network_streams_closed",
                "Streams closed",
                "participant");
        private readonly Gauge _streamsActive = MetricsCreator.CreateGauge(
            "network_streams_active",
            "Streams currently active",
            "participant");

        private readonly Counter _listenRequests = MetricsCreator.CreateCounter("network_listen_requests_total", "Listen requests", "protocol");
        private readonly Counter _connectRequests = MetricsCreator.CreateCounter("network_connect_requests_total", "Connect requests", "protocol");
        private readonly Counter _incomingConnections = MetricsCreator.CreateCounter("network_incoming_connections_total", "Incoming connections", "protocol");
        private readonly Counter _failedHandshakes = MetricsCreator.CreateCounter("network_failed_handshakes_total", "Failed handshakes");
        private readonly Counter _channelsConnectedCounter =
            MetricsCreator.CreateCounter(
                "network_channels_connected_total",
                "Number of channels opened",
                "participant");
        private readonly Counter _channelsDisconnectedCounter =
            MetricsCreator.CreateCounter(
                "network_channels_disconnected_total",
                "Number of channels closed",
                "participant");
        private readonly Gauge _channelsConnected = MetricsCreator.CreateGauge(
            "network_channels_connected",
            "Channels currently connected",
            "participant");
        private readonly Gauge _participantBandwidth = MetricsCreator.CreateGauge("network_participant_bandwidth", "Bandwidth per participant", "participant");
        private readonly Gauge _participantChannelIds =
            MetricsCreator.CreateGauge(
                "network_participant_channel_ids",
                "Channel numbers belonging to a participant",
                "participant",
                "no");
        private readonly Gauge _schedulerQueue = MetricsCreator.CreateGauge(
            "network_scheduler_queue",
            "Number of tasks queued in the scheduler");
        private readonly Counter _schedulerTasksExecuted = MetricsCreator.CreateCounter(
            "network_scheduler_tasks_total",
            "Total tasks executed by the scheduler");
        private readonly Gauge _schedulerWorkers = MetricsCreator.CreateGauge(
            "network_scheduler_workers",
            "Number of active scheduler workers");
        private readonly Gauge _schedulerLoad = MetricsCreator.CreateGauge(
            "network_scheduler_load",
            "Average number of tasks executed per second");
        private readonly Histogram _schedulerTaskDuration = MetricsCreator.CreateHistogram(
            "network_scheduler_task_seconds",
            "Duration of individual scheduler tasks in seconds");
        private readonly Histogram _schedulerTaskWait = MetricsCreator.CreateHistogram(
            "network_scheduler_task_wait_seconds",
            "Time tasks wait in the scheduler queue in seconds");
        private readonly Counter _schedulerTaskTimeouts = MetricsCreator.CreateCounter(
            "network_scheduler_task_timeouts_total",
            "Number of scheduler tasks that exceeded the configured timeout");
        private readonly Gauge _schedulerWorkerUtilization = MetricsCreator.CreateGauge(
            "network_scheduler_worker_utilization",
            "Ratio of active workers to worker limit");
        private readonly Gauge _schedulerLatency = MetricsCreator.CreateGauge(
            "network_scheduler_latency_seconds",
            "Average delay of tasks in the scheduler queue in seconds");
        private readonly Gauge _networkInfo;

        private readonly ConcurrentQueue<(DateTime time, string ev)> _events = new();
        private const int MaxEvents = 1000;
        private System.IO.StreamWriter? _eventWriter;

        private readonly Counter _channelSentBytes = MetricsCreator.CreateCounter(
            "network_channel_sent_bytes",
            "Bytes sent per channel",
            "participant",
            "channel");
        private readonly Counter _channelRecvBytes = MetricsCreator.CreateCounter(
            "network_channel_recv_bytes",
            "Bytes received per channel",
            "participant",
            "channel");
        private readonly Counter _channelSentMessages = MetricsCreator.CreateCounter(
            "network_channel_sent_messages",
            "Messages sent per channel",
            "participant",
            "channel");
        private readonly Counter _channelRecvMessages = MetricsCreator.CreateCounter(
            "network_channel_recv_messages",
            "Messages received per channel",
            "participant",
            "channel");

        private readonly Gauge _channelCongestion = MetricsCreator.CreateGauge(
            "network_channel_congestion",
            "Number of messages queued per channel",
            "participant",
            "channel");

        private readonly Gauge _streamRtt = MetricsCreator.CreateGauge(
            "network_stream_rtt_ms",
            "Last round trip time per stream in milliseconds",
            "participant",
            "stream");

        private readonly Gauge _streamRttAvg = MetricsCreator.CreateGauge(
            "network_stream_rtt_avg_ms",
            "Average round trip time per stream in milliseconds",
            "participant",
            "stream");

        private readonly Histogram _streamDelay = MetricsCreator.CreateHistogram(
            "network_stream_delay_ms",
            "Delivery delay per stream in milliseconds",
            "participant",
            "stream");

        private readonly Counter _streamLosses = MetricsCreator.CreateCounter(
            "network_stream_losses_total",
            "Number of lost messages per stream",
            "participant",
            "stream");

        private readonly System.Collections.Concurrent.ConcurrentDictionary<(string p, string s), (double sum, int count)> _rttStats = new();

        private readonly string _localPid;

        public Metrics(Pid localPid)
        {
            _localPid = localPid.ToString();
            _networkInfo = MetricsCreator.CreateGauge("network_info", "Static network information", "version", "local_pid");
            var version = $"{Protocol.Types.VELOREN_NETWORK_VERSION[0]}.{Protocol.Types.VELOREN_NETWORK_VERSION[1]}.{Protocol.Types.VELOREN_NETWORK_VERSION[2]}";
            _networkInfo.WithLabels(version, _localPid).Set(1);
        }

        private void Log(string message)
        {
            var entry = (DateTime.UtcNow, message);
            _events.Enqueue(entry);
            while (_events.Count > MaxEvents && _events.TryDequeue(out _)) { }
            if (_eventWriter != null)
            {
                _eventWriter.WriteLine($"{entry.Item1:O} {entry.Item2}");
            }
        }

        public IEnumerable<(DateTime time, string ev)> DrainEvents()
        {
            while (_events.TryDequeue(out var e))
                yield return e;
        }

        private long _sentBytes;
        private long _recvBytes;
        private long _sentMessages;
        private long _recvMessages;

        public void ParticipantConnected(Pid pid)
        {
            _participantsConnectedCounter.Inc();
            _participantsActive.Inc();
            Log($"participant {pid} connected");
        }

        public void ParticipantDisconnected(Pid pid)
        {
            _participantsDisconnectedCounter.Inc();
            _participantsActive.Dec();
            Log($"participant {pid} disconnected");
        }

        public void StreamOpened(Pid pid)
        {
            string p = pid.ToString();
            _streamsOpenedCounter.WithLabels(p).Inc();
            _streamsActive.WithLabels(p).Inc();
            Log($"stream opened for {pid}");
        }

        public void StreamClosed(Pid pid)
        {
            string p = pid.ToString();
            _streamsClosedCounter.WithLabels(p).Inc();
            _streamsActive.WithLabels(p).Dec();
            Log($"stream closed for {pid}");
        }

        public void ListenRequest(ListenAddr addr)
        {
            _listenRequests.WithLabels(ProtocolName(addr)).Inc();
            Log($"listen {ProtocolName(addr)}");
        }

        public void ConnectRequest(ConnectAddr addr)
        {
            _connectRequests.WithLabels(ProtocolName(addr)).Inc();
            Log($"connect {ProtocolName(addr)}");
        }

        public void IncomingConnection(ConnectAddr addr)
        {
            _incomingConnections.WithLabels(ProtocolName(addr)).Inc();
            Log($"incoming {ProtocolName(addr)}");
        }

        public void FailedHandshake()
        {
            _failedHandshakes.Inc();
            Log("handshake failed");
        }

        public void ChannelConnected(Pid pid, int index, Sid channelId)
        {
            string p = pid.ToString();
            _channelsConnectedCounter.WithLabels(p).Inc();
            _channelsConnected.WithLabels(p).Inc();
            _participantChannelIds.WithLabels(p, index.ToString()).Set((double)channelId.Value);
            _channelCongestion.WithLabels(p, channelId.Value.ToString()).Set(0);
            Log($"channel {channelId.Value} opened for {pid}");
        }

        public void ChannelDisconnected(Pid pid, int index)
        {
            string p = pid.ToString();
            _channelsDisconnectedCounter.WithLabels(p).Inc();
            _channelsConnected.WithLabels(p).Dec();
            _participantChannelIds.WithLabels(p, index.ToString()).Set(0);
            _channelCongestion.WithLabels(p, index.ToString()).Set(0);
            Log($"channel {index} closed for {pid}");
        }

        public void CleanupParticipant(Pid pid)
        {
            string p = pid.ToString();
            _channelsConnected.WithLabels(p).Set(0);
            _participantBandwidth.WithLabels(p).Set(0);
            _streamsActive.WithLabels(p).Set(0);
            for (int i = 0; i < 5; i++)
                _participantChannelIds.WithLabels(p, i.ToString()).Set(0);
            for (int i = 0; i < 5; i++)
                _channelCongestion.WithLabels(p, i.ToString()).Set(0);
        }

        public void ParticipantBandwidth(Pid pid, float value)
        {
            _participantBandwidth.WithLabels(pid.ToString()).Set(value);
        }

        public void ChannelSent(Pid pid, Sid channel, int bytes)
        {
            string p = pid.ToString();
            string c = channel.Value.ToString();
            _channelSentBytes.WithLabels(p, c).Inc(bytes);
            _channelSentMessages.WithLabels(p, c).Inc();
        }

        public void ChannelReceived(Pid pid, Sid channel, int bytes)
        {
            string p = pid.ToString();
            string c = channel.Value.ToString();
            _channelRecvBytes.WithLabels(p, c).Inc(bytes);
            _channelRecvMessages.WithLabels(p, c).Inc();
        }

        public void ChannelCongestion(Pid pid, Sid channel, int queueSize)
        {
            string p = pid.ToString();
            string c = channel.Value.ToString();
            _channelCongestion.WithLabels(p, c).Set(queueSize);
        }

        public void ChannelSentMessage(Pid pid, Sid channel)
        {
            string p = pid.ToString();
            string c = channel.Value.ToString();
            _channelSentMessages.WithLabels(p, c).Inc();
        }

        public void ChannelReceivedMessage(Pid pid, Sid channel)
        {
            string p = pid.ToString();
            string c = channel.Value.ToString();
            _channelRecvMessages.WithLabels(p, c).Inc();
        }

        public void CountSent(int bytes)
        {
            Interlocked.Add(ref _sentBytes, bytes);
            Interlocked.Increment(ref _sentMessages);
            _sentBytesCounter.Inc(bytes);
            _sentMessagesCounter.Inc();
        }

        public void CountReceived(int bytes)
        {
            Interlocked.Add(ref _recvBytes, bytes);
            Interlocked.Increment(ref _recvMessages);
            _recvBytesCounter.Inc(bytes);
            _recvMessagesCounter.Inc();
        }

        public (long sentBytes, long recvBytes, long sentMessages, long recvMessages) Snapshot()
            => (Interlocked.Read(ref _sentBytes), Interlocked.Read(ref _recvBytes),
                Interlocked.Read(ref _sentMessages), Interlocked.Read(ref _recvMessages));

        public void SchedulerQueued(int count)
            => _schedulerQueue.Set(count);

        public void SchedulerTaskExecuted()
            => _schedulerTasksExecuted.Inc();

        public void SchedulerWorkers(int count)
            => _schedulerWorkers.Set(count);

        public void SchedulerLoad(double value)
            => _schedulerLoad.Set(value);

        public void SchedulerTaskDuration(double seconds)
            => _schedulerTaskDuration.Observe(seconds);

        public void SchedulerTaskWaitTime(double seconds)
            => _schedulerTaskWait.Observe(seconds);

        public void SchedulerTaskTimeout()
            => _schedulerTaskTimeouts.Inc();

        public void SchedulerWorkerUtilization(double value)
            => _schedulerWorkerUtilization.Set(value);

        public void SchedulerLatency(double seconds)
            => _schedulerLatency.Set(seconds);

        public void StreamRtt(Pid pid, Sid stream, double ms)
        {
            string p = pid.ToString();
            string s = stream.Value.ToString();
            _streamRtt.WithLabels(p, s).Set(ms);
            var key = (p, s);
            var (sum, count) = _rttStats.AddOrUpdate(key, (_sum: ms, _count: 1), (k, v) => (v.sum + ms, v.count + 1));
            _streamRttAvg.WithLabels(p, s).Set(sum / count);
            _streamDelay.WithLabels(p, s).Observe(ms);
        }

        public void StreamRttReset(Pid pid, Sid stream)
        {
            string p = pid.ToString();
            string s = stream.Value.ToString();
            _streamRtt.WithLabels(p, s).Set(0);
            _streamRttAvg.WithLabels(p, s).Set(0);
            _rttStats.TryRemove((p, s), out _);
        }

        public void StreamLoss(Pid pid, Sid stream)
        {
            string p = pid.ToString();
            string s = stream.Value.ToString();
            _streamLosses.WithLabels(p, s).Inc();
        }

        private MetricServer? _metricServer;

        public void StartEventLog(string path)
        {
            _eventWriter = new System.IO.StreamWriter(path, append: true);
            _eventWriter.AutoFlush = true;
        }

        public void StopEventLog()
        {
            _eventWriter?.Dispose();
            _eventWriter = null;
        }

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

        public void StartPrometheus(int port = 9091)
        {
            if (_metricServer != null) return;
            _metricServer = new MetricServer(port: port);
            _metricServer.Start();
        }

        public void StopPrometheus() => _metricServer?.StopAsync().GetAwaiter().GetResult();
    }
}
