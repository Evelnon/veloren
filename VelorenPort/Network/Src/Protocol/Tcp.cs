using System.Net.Sockets;
using System.Threading.Tasks;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Very small helper around <see cref="TcpClient"/> for the migration stage.
    /// </summary>
    public static class Tcp {
        public static async Task SendAsync(TcpClient client, byte[] data) => await client.GetStream().WriteAsync(data);
        public static async Task<int> ReceiveAsync(TcpClient client, byte[] buffer) => await client.GetStream().ReadAsync(buffer);
    }
}
