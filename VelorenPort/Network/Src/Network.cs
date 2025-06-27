using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Manejador de red para Unity. Mantiene una lista de participantes
    /// conectados y permite iniciar futuras conexiones mediante sockets.
    /// </summary>
    public class Network {
        public Pid LocalPid { get; }
        private readonly ConcurrentQueue<Participant> _pending = new();
        private readonly ConcurrentDictionary<Pid, Participant> _participants = new();

        public Network(Pid pid) {
            LocalPid = pid;
        }

        public Task ListenAsync(ListenAddr addr) {
            // Actual socket handling will be plugged in here.
            return Task.CompletedTask;
        }

        public Task<Participant> ConnectAsync(ConnectAddr addr) {
            // Crea la conexion y devuelve un participante asociado. En futuras
            // revisiones se integrará el uso de sockets reales.
            var remote = new Participant(Pid.NewPid(), addr);
            _participants[remote.Id] = remote;
            _pending.Enqueue(remote);
            return Task.FromResult(remote);
        }

        public Task<Participant?> ConnectedAsync() {
            _pending.TryDequeue(out var participant);
            return Task.FromResult(participant);
        }

        public bool TryGetParticipant(Pid id, out Participant participant)
            => _participants.TryGetValue(id, out participant);
    }
}
