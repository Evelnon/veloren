using System;
using System.Net;

namespace VelorenPort.Network {
    public enum AddrType {
        Tcp,
        Udp,
        Quic,
        Mpsc
    }

    [Serializable]
    public class ConnectAddr {
        public AddrType Type { get; private set; }
        public IPEndPoint? EndPoint { get; private set; }
        public ulong ChannelId { get; private set; }

        private ConnectAddr(AddrType type, IPEndPoint? endPoint, ulong channelId) {
            Type = type;
            EndPoint = endPoint;
            ChannelId = channelId;
        }

        public static ConnectAddr Tcp(IPEndPoint ep) => new ConnectAddr(AddrType.Tcp, ep, 0);
        public static ConnectAddr Udp(IPEndPoint ep) => new ConnectAddr(AddrType.Udp, ep, 0);
        public static ConnectAddr Quic(IPEndPoint ep) => new ConnectAddr(AddrType.Quic, ep, 0);
        public static ConnectAddr Mpsc(ulong id) => new ConnectAddr(AddrType.Mpsc, null, id);

        public override string ToString() => Type switch {
            AddrType.Mpsc => $"Mpsc({ChannelId})",
            _ => $"{Type}({EndPoint})"
        };
    }
}
