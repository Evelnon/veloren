using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Esqueleto de manejador de red para Unity. Por ahora solo simula
    /// conexiones TCP y mantiene la cola de participantes conectados.
    /// </summary>
    public class Network {
        public Pid LocalPid { get; }
        private readonly ConcurrentQueue<Participant> _pending = new();
        private readonly ConcurrentDictionary<Pid, Participant> _participants = new();

        public Network(Pid pid) {
            LocalPid = pid;
        }

        public Task ListenAsync(ListenAddr addr) {
            // TODO: implementar sockets reales. Por ahora no hace nada.
            return Task.CompletedTask;
        }

        public Task<Participant> ConnectAsync(ConnectAddr addr) {
            // Simula una conexion inmediata y retorna un participante nuevo
            var remote = new Participant(Pid.NewPid());
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
