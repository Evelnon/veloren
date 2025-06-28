namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Minimal metrics container used while migrating from Rust.
    /// </summary>
    public class Metrics {
        private ulong _sentBytes;
        private ulong _receivedBytes;

        public void RecordSent(ulong bytes) => _sentBytes += bytes;
        public void RecordReceived(ulong bytes) => _receivedBytes += bytes;
        public ulong SentBytes => _sentBytes;
        public ulong ReceivedBytes => _receivedBytes;
    }
}
