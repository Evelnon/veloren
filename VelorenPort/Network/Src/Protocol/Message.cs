namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Container for protocol payloads exchanged over streams. Stores the
    /// message id and stream id so frames map one-to-one with the Rust
    /// OTMessage/ITMessage structures.
    /// </summary>
    public class Message {
        public byte[] Data { get; }
        public ulong Mid { get; }
        public Sid Sid { get; }

        public Message(byte[] data, ulong mid, Sid sid) {
            Data = data;
            Mid = mid;
            Sid = sid;
        }
    }
}
