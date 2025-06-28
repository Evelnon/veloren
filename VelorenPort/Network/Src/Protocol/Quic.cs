using System.Net.Quic;
using System.Threading.Tasks;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Helper methods for working with <see cref="QuicConnection"/> during migration.
    /// </summary>
    public static class Quic {
        public static async Task<QuicStream> OpenStreamAsync(QuicConnection conn) => await conn.OpenOutboundStreamAsync();
        public static async Task SendAsync(QuicStream stream, byte[] data) => await stream.WriteAsync(data);
    }
}
