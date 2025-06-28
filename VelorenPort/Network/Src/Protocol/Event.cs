namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Events exchanged between protocol implementations and channels.
    /// </summary>
    public abstract record ProtocolEvent {
        public sealed record Shutdown : ProtocolEvent;
        public sealed record OpenStream(Sid Sid, byte Prio, Promises Promises, ulong GuaranteedBandwidth) : ProtocolEvent;
        public sealed record CloseStream(Sid Sid) : ProtocolEvent;
        public sealed record Message(byte[] Data, Sid Sid) : ProtocolEvent;
    }
}
