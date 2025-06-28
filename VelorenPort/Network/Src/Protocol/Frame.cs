using System;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Frames exchanged at the protocol level. These are simplified versions of
    /// the Rust definitions and are used during the handshake and data exchange.
    /// </summary>
    public abstract record InitFrame {
        public sealed record Handshake(byte[] MagicNumber, uint[] Version) : InitFrame;
        public sealed record Init(Pid Pid, Guid Secret) : InitFrame;
        public sealed record Raw(byte[] Data) : InitFrame;
    }

    public abstract record OTFrame {
        public sealed record Shutdown : OTFrame;
        public sealed record OpenStream(Sid Sid, byte Prio, Promises Promises, ulong GuaranteedBandwidth) : OTFrame;
        public sealed record CloseStream(Sid Sid) : OTFrame;
        public sealed record DataHeader(ulong Mid, Sid Sid, ulong Length) : OTFrame;
        public sealed record Data(ulong Mid, byte[] Data) : OTFrame;
    }

    public abstract record ITFrame {
        public sealed record Shutdown : ITFrame;
        public sealed record OpenStream(Sid Sid, byte Prio, Promises Promises, ulong GuaranteedBandwidth) : ITFrame;
        public sealed record CloseStream(Sid Sid) : ITFrame;
        public sealed record DataHeader(ulong Mid, Sid Sid, ulong Length) : ITFrame;
        public sealed record Data(ulong Mid, byte[] Data) : ITFrame;
    }
}
