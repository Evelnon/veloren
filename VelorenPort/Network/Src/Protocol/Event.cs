using System;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Events exchanged between protocol implementations and channels.
    /// </summary>
    public abstract record ProtocolEvent {
        public sealed record Shutdown : ProtocolEvent;
        public sealed record OpenStream(Sid Sid, byte Prio, Promises Promises, ulong GuaranteedBandwidth) : ProtocolEvent;
        public sealed record CloseStream(Sid Sid) : ProtocolEvent;
        public sealed record Message(byte[] Data, Sid Sid) : ProtocolEvent;

        /// <summary>
        /// Convert this high level event into a protocol frame.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when converting a
        /// <see cref="Message"/> event since it maps to multiple frames.</exception>
        public OTFrame ToFrame() => this switch {
            Shutdown => new OTFrame.Shutdown(),
            OpenStream(var sid, var prio, var promises, var bw) =>
                new OTFrame.OpenStream(sid, prio, promises, bw),
            CloseStream(var sid) => new OTFrame.CloseStream(sid),
            Message => throw new InvalidOperationException("Message events span multiple frames"),
            _ => throw new InvalidOperationException()
        };
    }
}
