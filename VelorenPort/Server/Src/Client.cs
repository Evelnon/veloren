
using VelorenPort.Network;

namespace VelorenPort.Server {
    /// <summary>
    /// Represents a connected game client with its participant handle.
    /// Additional per-client state will be added as systems are ported.
    /// </summary>
    public class Client {
        public Participant Participant { get; }

        internal Client(Participant participant) {
            Participant = participant;
        }
    }
}
