using System.Net.Sockets;
using System.Text;

namespace VelorenPort.Network {
    /// <summary>
    /// Funciones utilitarias para enviar mensajes UDP sin depender de Rust.
    /// </summary>
    public static class UdpWrapper {
        /// <summary>
        /// Envia un mensaje UDP utilizando la implementación nativa de C#.
        /// </summary>
        public static bool Send(string address, string message) {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            try {
                string[] parts = address.Split(':');
                if (parts.Length != 2) {
                    return false;
                }
                string host = parts[0];
                int port = int.Parse(parts[1]);

                using UdpClient client = new();
                client.Send(bytes, bytes.Length, host, port);
                return true;
            } catch {
                return false;
            }
        }
    }
}
