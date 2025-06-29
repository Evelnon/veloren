using System.Text;

namespace VelorenPort.Network {
    /// <summary>
    /// Funciones utilitarias para interactuar con la biblioteca Rust.
    /// </summary>
    public static class UdpWrapper {
        /// <summary>
        /// Envia un mensaje UDP utilizando la implementacion Rust.
        /// </summary>
        public static bool Send(string address, string message) {
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            int sent = RustBindings.SendUdp(address, bytes, (ulong)bytes.Length);
            return sent == bytes.Length;
        }
    }
}
