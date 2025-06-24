using System;
using System.Net;

namespace VelorenPort.Network {
    [Serializable]
    public class ListenAddr {
        public AddrType Type { get; private set; }
        public IPEndPoint? EndPoint { get; private set; }
        public ulong ChannelId { get; private set; }

        private ListenAddr(AddrType type, IPEndPoint? endPoint, ulong channelId) {
            Type = type;
            EndPoint = endPoint;
            ChannelId = channelId;
        }

        public static ListenAddr Tcp(IPEndPoint ep) => new ListenAddr(AddrType.Tcp, ep, 0);
        public static ListenAddr Udp(IPEndPoint ep) => new ListenAddr(AddrType.Udp, ep, 0);
        public static ListenAddr Quic(IPEndPoint ep) => new ListenAddr(AddrType.Quic, ep, 0);
        public static ListenAddr Mpsc(ulong id) => new ListenAddr(AddrType.Mpsc, null, id);

        public override string ToString() => Type switch {
            AddrType.Mpsc => $"Mpsc({ChannelId})",
            _ => $"{Type}({EndPoint})"
        };
    }
}
