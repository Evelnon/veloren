using System;

namespace VelorenPort.Network {
    [Serializable]
    public class ParticipantEvent {
        public enum EventType {
            ChannelCreated,
            ChannelDeleted
        }

        public EventType Type { get; private set; }
        public ConnectAddr Address { get; private set; }

        private ParticipantEvent(EventType type, ConnectAddr addr) {
            Type = type;
            Address = addr;
        }

        public static ParticipantEvent ChannelCreated(ConnectAddr addr) => new ParticipantEvent(EventType.ChannelCreated, addr);
        public static ParticipantEvent ChannelDeleted(ConnectAddr addr) => new ParticipantEvent(EventType.ChannelDeleted, addr);
    }
}
