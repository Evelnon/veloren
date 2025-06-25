using System;
using System.Net;

namespace VelorenPort.Network {
    /// <summary>
    /// Direcciones válidas para escuchar conexiones entrantes.
    /// Equivale al enum <c>ListenAddr</c> de la implementación en Rust.
    /// </summary>
    [Serializable]
    public abstract record ListenAddr {
        public sealed record Tcp(IPEndPoint EndPoint) : ListenAddr;
        public sealed record Udp(IPEndPoint EndPoint) : ListenAddr;
        public sealed record Quic(IPEndPoint EndPoint, QuicServerConfig Config) : ListenAddr;
        public sealed record Mpsc(ulong ChannelId) : ListenAddr;
    }
}
