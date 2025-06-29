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
        public sealed record Data(ulong Mid, byte[] Payload) : OTFrame;
    }

public abstract record ITFrame {
        public sealed record Shutdown : ITFrame;
        public sealed record OpenStream(Sid Sid, byte Prio, Promises Promises, ulong GuaranteedBandwidth) : ITFrame;
        public sealed record CloseStream(Sid Sid) : ITFrame;
        public sealed record DataHeader(ulong Mid, Sid Sid, ulong Length) : ITFrame;
        public sealed record Data(ulong Mid, byte[] Payload) : ITFrame;
    }

    /// <summary>
    /// Helpers for converting frames to and from the byte representation used by
    /// the Rust implementation. This only covers a subset of the protocol but
    /// allows interop in tests.
    /// </summary>
    public static class FrameCodec {
        private const byte FrameHandshake = 1;
        private const byte FrameInit = 2;
        private const byte FrameShutdown = 3;
        private const byte FrameOpenStream = 4;
        private const byte FrameCloseStream = 5;
        private const byte FrameDataHeader = 6;
        private const byte FrameData = 7;
        private const byte FrameRaw = 8;

        public static byte[] Serialize(InitFrame frame) {
            using var ms = new System.IO.MemoryStream();
            switch (frame) {
                case InitFrame.Handshake(var magic, var ver):
                    ms.WriteByte(FrameHandshake);
                    ms.Write(magic, 0, 7);
                    ms.Write(System.BitConverter.GetBytes(ver[0]));
                    ms.Write(System.BitConverter.GetBytes(ver[1]));
                    ms.Write(System.BitConverter.GetBytes(ver[2]));
                    break;
                case InitFrame.Init(var pid, var secret):
                    ms.WriteByte(FrameInit);
                    ms.Write(pid.ToByteArray(), 0, 16);
                    ms.Write(secret.ToByteArray(), 0, 16);
                    break;
                case InitFrame.Raw(var data):
                    ms.WriteByte(FrameRaw);
                    ms.Write(System.BitConverter.GetBytes((ushort)data.Length));
                    ms.Write(data, 0, data.Length);
                    break;
            }
            return ms.ToArray();
        }

        public static bool TryDeserializeInit(ReadOnlySpan<byte> data, out InitFrame? frame, out int consumed) {
            frame = null;
            consumed = 0;
            if (data.IsEmpty) return false;
            switch (data[0]) {
                case FrameHandshake:
                    if (data.Length < 1 + 7 + 12) return false;
                    var magic = data.Slice(1, 7).ToArray();
                    uint v0 = System.BitConverter.ToUInt32(data.Slice(8, 4));
                    uint v1 = System.BitConverter.ToUInt32(data.Slice(12, 4));
                    uint v2 = System.BitConverter.ToUInt32(data.Slice(16, 4));
                    frame = new InitFrame.Handshake(magic, new[] { v0, v1, v2 });
                    consumed = 1 + 7 + 12;
                    return true;
                case FrameInit:
                    if (data.Length < 1 + 32) return false;
                    var pid = new Pid(new System.Guid(data.Slice(1, 16)));
                    var secret = new System.Guid(data.Slice(17, 16));
                    frame = new InitFrame.Init(pid, secret);
                    consumed = 1 + 32;
                    return true;
                case FrameRaw:
                    if (data.Length < 1 + 2) return false;
                    ushort len = System.BitConverter.ToUInt16(data.Slice(1, 2));
                    if (data.Length < 1 + 2 + len) return false;
                    frame = new InitFrame.Raw(data.Slice(3, len).ToArray());
                    consumed = 1 + 2 + len;
                    return true;
                default:
                    return false;
            }
        }

        public static byte[] Serialize(OTFrame frame) {
            using var ms = new System.IO.MemoryStream();
            switch (frame) {
                case OTFrame.Shutdown:
                    ms.WriteByte(FrameShutdown);
                    break;
                case OTFrame.OpenStream(var sid, var prio, var promises, var bw):
                    ms.WriteByte(FrameOpenStream);
                    ms.Write(System.BitConverter.GetBytes(sid.Value));
                    ms.WriteByte(prio);
                    ms.WriteByte((byte)promises);
                    ms.Write(System.BitConverter.GetBytes(bw));
                    break;
                case OTFrame.CloseStream(var sid):
                    ms.WriteByte(FrameCloseStream);
                    ms.Write(System.BitConverter.GetBytes(sid.Value));
                    break;
                case OTFrame.DataHeader(var mid, var sid, var len):
                    ms.WriteByte(FrameDataHeader);
                    ms.Write(System.BitConverter.GetBytes(mid));
                    ms.Write(System.BitConverter.GetBytes(sid.Value));
                    ms.Write(System.BitConverter.GetBytes(len));
                    break;
                case OTFrame.Data(var mid, var payload):
                    ms.WriteByte(FrameData);
                    ms.Write(System.BitConverter.GetBytes(mid));
                    ms.Write(System.BitConverter.GetBytes((ushort)payload.Length));
                    ms.Write(payload, 0, payload.Length);
                    break;
            }
            return ms.ToArray();
        }

        public static bool TryDeserializeIT(ReadOnlySpan<byte> data, out ITFrame? frame, out int consumed) {
            frame = null;
            consumed = 0;
            if (data.IsEmpty) return false;
            switch (data[0]) {
                case FrameShutdown:
                    frame = new ITFrame.Shutdown();
                    consumed = 1;
                    return true;
                case FrameOpenStream:
                    if (data.Length < 1 + 8 + 1 + 1 + 8) return false;
                    ulong sidVal = System.BitConverter.ToUInt64(data.Slice(1, 8));
                    byte prio = data[9];
                    var promises = (Promises)data[10];
                    ulong bw = System.BitConverter.ToUInt64(data.Slice(11, 8));
                    frame = new ITFrame.OpenStream(new Sid(sidVal), prio, promises, bw);
                    consumed = 1 + 8 + 1 + 1 + 8;
                    return true;
                case FrameCloseStream:
                    if (data.Length < 1 + 8) return false;
                    ulong csid = System.BitConverter.ToUInt64(data.Slice(1, 8));
                    frame = new ITFrame.CloseStream(new Sid(csid));
                    consumed = 1 + 8;
                    return true;
                case FrameDataHeader:
                    if (data.Length < 1 + 8 + 8 + 8) return false;
                    ulong mid = System.BitConverter.ToUInt64(data.Slice(1, 8));
                    ulong dsid = System.BitConverter.ToUInt64(data.Slice(9, 8));
                    ulong length = System.BitConverter.ToUInt64(data.Slice(17, 8));
                    frame = new ITFrame.DataHeader(mid, new Sid(dsid), length);
                    consumed = 1 + 8 + 8 + 8;
                    return true;
                case FrameData:
                    if (data.Length < 1 + 8 + 2) return false;
                    ulong mid2 = System.BitConverter.ToUInt64(data.Slice(1, 8));
                    ushort l = System.BitConverter.ToUInt16(data.Slice(9, 2));
                    if (data.Length < 1 + 8 + 2 + l) return false;
                    frame = new ITFrame.Data(mid2, data.Slice(11, l).ToArray());
                    consumed = 1 + 8 + 2 + l;
                    return true;
                default:
                    return false;
            }
        }
    }
}
