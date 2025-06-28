using System;
using System.Text;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Shared constants and simple types used by the network protocol.
    /// </summary>
    public static class Types {
        public static readonly byte[] VELOREN_MAGIC_NUMBER = Encoding.ASCII.GetBytes("VELOREN");
        public static readonly uint[] VELOREN_NETWORK_VERSION = { 0u, 6u, 0u };

        public static readonly Sid STREAM_ID_OFFSET1 = new Sid(0);
        public static readonly Sid STREAM_ID_OFFSET2 = new Sid(ulong.MaxValue / 2);

        public const byte HIGHEST_PRIO = 7;
    }

    /// <summary>Message identifier.</summary>
    public readonly struct Mid {
        public readonly ulong Value;
        public Mid(ulong value) { Value = value; }
    }

    /// <summary>Channel identifier.</summary>
    public readonly struct Cid {
        public readonly ulong Value;
        public Cid(ulong value) { Value = value; }
    }

    /// <summary>Bandwidth value in bytes per second.</summary>
    public readonly struct Bandwidth {
        public readonly ulong Value;
        public Bandwidth(ulong value) { Value = value; }
    }
}
