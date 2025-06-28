using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Simple helper methods for working with <see cref="UdpClient"/>.
    /// </summary>
    public static class Udp {
        public static async Task SendAsync(UdpClient client, IPEndPoint ep, byte[] data) => await client.SendAsync(data, data.Length, ep);
        public static async Task<UdpReceiveResult> ReceiveAsync(UdpClient client) => await client.ReceiveAsync();
    }
}
