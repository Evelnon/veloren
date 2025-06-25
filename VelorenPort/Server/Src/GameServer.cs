using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VelorenPort.CoreEngine;
using VelorenPort.Network;
using VelorenPort.World;

namespace VelorenPort.Server {
    /// <summary>
    /// Coordinates the main game loop and manages connected clients. This
    /// version aims to mirror the high level behaviour of the Rust server
    /// crate using idiomatic C# constructs.
    /// </summary>
    public class GameServer {
        public Network.Network Network { get; }
        public Clock Clock { get; }
        public WorldIndex WorldIndex { get; }

        private readonly List<Client> _clients = new();
        private readonly ConnectionHandler _connections;

        public GameServer(Pid pid, TimeSpan tickRate, uint worldSeed) {
            Network = new Network.Network(pid);
            Clock = new Clock(tickRate);
            WorldIndex = new WorldIndex(worldSeed);
            _connections = new ConnectionHandler(Network);
        }

        /// <summary>
        /// Listen for connections and start the main loop until the
        /// provided cancellation token is signalled.
        /// </summary>
        public async Task RunAsync(ListenAddr addr, CancellationToken token) {
            var connectionTask = _connections.RunAsync(addr, token);
            while (!token.IsCancellationRequested) {
                Clock.Tick();
                AcceptNewClients();
                UpdateWorld();
                await Task.Yield();
            }
            await connectionTask;
        }

        private void AcceptNewClients() {
            while (_connections.TryDequeue(out var client)) {
                _clients.Add(client);
            }
        }

        private void UpdateWorld() {
            WorldIndex.Time += (float)Clock.Dt.TotalSeconds;

            foreach (var client in _clients) {
                RegionSubscriptionUpdater.UpdateSubscription(
                    client.Position,
                    client.Presence,
                    client.RegionSubscription);
            }
        }
    }
}
