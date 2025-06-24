using System.Collections.Concurrent;

namespace VelorenPort.Network {
    /// <summary>
    /// Represents a remote participant with multiple channels.
    /// </summary>
    public class Participant {
        public Pid Id { get; }
        private readonly ConcurrentDictionary<Sid, Channel> _channels = new();

        internal Participant(Pid id) {
            Id = id;
        }

        public Channel OpenChannel(Sid id, StreamParams parameters) {
            var ch = new Channel(id);
            _channels[id] = ch;
            return ch;
        }

        public bool TryGetChannel(Sid id, out Channel channel) => _channels.TryGetValue(id, out channel);
        internal void CloseChannel(Sid id) => _channels.TryRemove(id, out _);
    }
}
