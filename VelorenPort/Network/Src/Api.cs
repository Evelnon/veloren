using System;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// High level API exposing network functionality for the Unity client.
    /// Wraps Network and scheduler usage.
    /// </summary>
    public class Api {
        private readonly Network _network;
        private readonly Scheduler _scheduler;

        public Api(Pid pid, int schedulerWorkers = 0) {
            _network = new Network(pid);
            _scheduler = new Scheduler(_network.Metrics, schedulerWorkers);
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

        public Task ListenAsync(ListenAddr addr) => _network.ListenAsync(addr);

        public void StopListening() => _network.StopListening();

        public Task<Participant> ConnectAsync(ConnectAddr addr) => _network.ConnectAsync(addr);

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

        public async Task ShutdownAsync()
        {
            await _network.ShutdownAsync();
            await _scheduler.StopAsync();
        }
    }
}
