using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VelorenPort.Network;

namespace VelorenPort.Server {
    /// <summary>
    /// Background task that listens for new participants and converts them
    /// into <see cref="Client"/> instances for the server to consume.
    /// </summary>
    public class ConnectionHandler {
        private readonly Network.Network _network;
        private readonly ConcurrentQueue<Client> _pending = new();

        public ConnectionHandler(Network.Network network) {
            _network = network;
        }

        /// <summary>
        /// Run the connection loop until the cancellation token is signaled.
        /// </summary>
        public async Task RunAsync(ListenAddr addr, CancellationToken token) {
            await _network.ListenAsync(addr);
            while (!token.IsCancellationRequested) {
                var participant = await _network.ConnectedAsync();
                if (participant != null) {
                    _pending.Enqueue(new Client(participant));
                }
            }
        }

        public bool TryDequeue(out Client client) => _pending.TryDequeue(out client);
    }
}
