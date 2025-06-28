using System.Threading.Channels;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Simple MPSC (multi-producer single-consumer) protocol implementation used during migration.
    /// </summary>
    public class Mpsc {
        private readonly Channel<byte[]> _channel = Channel.CreateUnbounded<byte[]>();
        public ChannelWriter<byte[]> Sender => _channel.Writer;
        public ChannelReader<byte[]> Receiver => _channel.Reader;
    }
}
