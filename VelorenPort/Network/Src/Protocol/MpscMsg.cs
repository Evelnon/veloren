using System;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Message wrapper used by the in-memory MPSC transport.
    /// Mirrors the Rust <c>MpscMsg</c> enum so the serialized format matches.
    /// </summary>
    public abstract record MpscMsg {
        public sealed record Event(ProtocolEvent Event) : MpscMsg;
        public sealed record InitFrame(InitFrame Frame) : MpscMsg;
    }
}
