using System;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// High level API exposing network functionality for the Unity client.
    /// Wraps Network and scheduler usage.
    /// </summary>
    public class Api {
        private readonly Network _network;
        private readonly Scheduler _scheduler = new();

        public Api(Pid pid) {
            _network = new Network(pid);
        }

        public Task ListenAsync(ListenAddr addr) => _network.ListenAsync(addr);

        public Task<Participant> ConnectAsync(ConnectAddr addr) => _network.ConnectAsync(addr);

        public void Schedule(Func<Task> task) => _scheduler.Schedule(task);
    }
}
