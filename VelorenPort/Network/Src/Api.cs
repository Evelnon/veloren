using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace VelorenPort.Network {
    /// <summary>
    /// High level API exposing network functionality for clients.
    /// Wraps <see cref="Network"/> and scheduler usage.
    /// </summary>
    public class Api {
        private readonly Network _network;
        private readonly Scheduler _scheduler;

        public Api(Pid pid, int schedulerWorkers = 0, bool autoScaleScheduler = false) {
            _network = new Network(pid);
            _scheduler = new Scheduler(_network.Metrics, schedulerWorkers, autoScaleScheduler);
        }

        public event Action<Participant>? ParticipantConnected
        {
            add => _network.ParticipantConnected += value;
            remove => _network.ParticipantConnected -= value;
        }

        public event Action<Pid>? ParticipantDisconnected
        {
            add => _network.ParticipantDisconnected += value;
            remove => _network.ParticipantDisconnected -= value;
        }

        public Task ListenAsync(ListenAddr addr, HandshakeFeatures features = HandshakeFeatures.None)
            => _network.ListenAsync(addr, features);

        public void StopListening() => _network.StopListening();

        public Task<Participant> ConnectAsync(ConnectAddr addr, HandshakeFeatures features = HandshakeFeatures.None)
            => _network.ConnectAsync(addr, features);

        public bool TryGetParticipant(Pid pid, out Participant participant) =>
            _network.TryGetParticipant(pid, out participant);

        public void Schedule(Func<Task> task) => _scheduler.Schedule(task);

        public Task DisconnectAsync(Pid pid) => _network.DisconnectAsync(pid);
        public IEnumerable<Pid> Participants() => _network.ListParticipants();
        public bool TryGetParticipantStats(Pid pid, out (long sentBytes, long recvBytes) stats)
            => _network.TryGetParticipantStats(pid, out stats);
        public (long sentBytes, long recvBytes, long sentMessages, long recvMessages) MetricsSnapshot() => _network.MetricsSnapshot();

        public void StartMetrics(int port = 9091) => _network.StartMetrics(port);

        public void StopMetrics() => _network.StopMetrics();

        public void StartEventLog(string path) => _network.Metrics.StartEventLog(path);
        public void StopEventLog() => _network.Metrics.StopEventLog();

        public async Task ShutdownAsync(bool drainScheduler = true)
        {
            await _network.ShutdownAsync();
            await _scheduler.StopAsync(drainScheduler);
        }

        public void SetStreamPriorityWeights(int[] weights)
            => Stream.SetPriorityWeights(weights);

        public void SetSchedulerWorkers(int workers)
            => _scheduler.SetMaxWorkers(workers);

        public void EnableSchedulerAutoScale(bool enable)
            => _scheduler.EnableAutoScale(enable);
    }
}
