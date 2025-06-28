namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Simple protocol message container.
    /// </summary>
    public class Message {
        public byte[] Data { get; }
        public Message(byte[] data) { Data = data; }
    }
}
