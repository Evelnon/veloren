using System;
using System.Net;

namespace VelorenPort.Network {
    /// <summary>
    /// Representa una dirección de conexión. Sigue la semántica del enum
    /// <c>ConnectAddr</c> del proyecto original en Rust, incluyendo la variante
    /// QUIC con su configuración específica.
    /// </summary>
    [Serializable]
    public abstract record ConnectAddr {
        public sealed record Tcp(IPEndPoint EndPoint) : ConnectAddr;
        public sealed record Udp(IPEndPoint EndPoint) : ConnectAddr;
        public sealed record Quic(IPEndPoint EndPoint, QuicClientConfig Config, string Name) : ConnectAddr;
        public sealed record Mpsc(ulong ChannelId) : ConnectAddr;

        /// <summary>
        /// Devuelve la dirección IP si aplica, de lo contrario <c>null</c> para la variante Mpsc.
        /// </summary>
        public IPEndPoint? SocketAddr => this switch {
            Tcp t => t.EndPoint,
            Udp u => u.EndPoint,
            Quic q => q.EndPoint,
            _ => null
        };
    }
}
