using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VelorenPort.Network.Protocol;

namespace VelorenPort.Network {
    /// <summary>
    /// Implements a minimal handshake verifying magic number and network version.
    /// Mirrors part of the Rust handshake.rs logic.
    /// </summary>
    internal static class Handshake {
        internal static readonly byte[] MagicNumber = Encoding.ASCII.GetBytes("VELOREN");
        internal static readonly uint[] SupportedVersion = { 0u, 6u, 0u };

        private static int HeaderSize => MagicNumber.Length + SupportedVersion.Length * 4;
        private static int InitSize => 16 + 16 + 4;
        private static int HandshakeSize => HeaderSize + InitSize;
        private const byte AckByte = 0xAC;

        private enum HandshakeStep
        {
            SendHeader,
            ReceiveHeader,
            SendInit,
            ReceiveInit,
            SendAck,
            ReceiveAck,
            Complete
        }

        public static async Task<(Pid remotePid, Guid remoteSecret, HandshakeFeatures features, uint[] remoteVersion, Sid localOffset)> PerformAsync(
            Stream stream,
            bool initiator,
            Pid localPid,
            Guid localSecret,
            HandshakeFeatures localFeatures = HandshakeFeatures.None,
            CancellationToken token = default)
        {
            var header = new byte[HeaderSize];
            var init = new byte[InitSize];
            var ackBuf = new byte[1];

            WriteHeader(header);
            WriteInit(init, localPid, localSecret, localFeatures);

            uint[] version = Array.Empty<uint>();
            Pid pid = default;
            Guid secret = default;
            HandshakeFeatures remoteFlags = HandshakeFeatures.None;

            bool isInitiator = initiator;
            var step = initiator ? HandshakeStep.SendHeader : HandshakeStep.ReceiveHeader;
            while (step != HandshakeStep.Complete)
            {
                switch (step)
                {
                    case HandshakeStep.SendHeader:
                        await stream.WriteAsync(header, 0, header.Length, token);
                        await stream.FlushAsync(token);
                        step = HandshakeStep.ReceiveHeader;
                        break;
                    case HandshakeStep.ReceiveHeader:
                        await ReadExactAsync(stream, header, header.Length, token);
                        version = ValidateHeader(header);
                        if (!isInitiator)
                        {
                            step = HandshakeStep.SendHeader;
                        }
                        else
                        {
                            step = HandshakeStep.SendInit;
                        }
                        break;
                    case HandshakeStep.SendInit:
                        await stream.WriteAsync(init, 0, init.Length, token);
                        await stream.FlushAsync(token);
                        step = HandshakeStep.ReceiveInit;
                        break;
                    case HandshakeStep.ReceiveInit:
                        await ReadExactAsync(stream, init, init.Length, token);
                        (pid, secret, remoteFlags) = ValidateInit(init);
                        if (!isInitiator)
                        {
                            step = HandshakeStep.SendInit;
                        }
                        else
                        {
                            step = HandshakeStep.SendAck;
                        }
                        break;
                    case HandshakeStep.SendAck:
                        await stream.WriteAsync(new[] { AckByte }, 0, 1, token);
                        await stream.FlushAsync(token);
                        step = isInitiator ? HandshakeStep.ReceiveAck : HandshakeStep.Complete;
                        break;
                    case HandshakeStep.ReceiveAck:
                        await ReadExactAsync(stream, ackBuf, 1, token);
                        if (ackBuf[0] != AckByte)
                            throw new NetworkConnectError.Handshake(new InitProtocolError<ProtocolsError>.NotHandshake());
                        step = isInitiator ? HandshakeStep.Complete : HandshakeStep.SendAck;
                        break;
                }
            }

            var offset = initiator ? Types.STREAM_ID_OFFSET1 : Types.STREAM_ID_OFFSET2;
            var negotiated = remoteFlags & localFeatures;
            return (pid, secret, negotiated, version, offset);
        }

        private static void WriteHeader(byte[] buffer, int offset = 0) {
            Array.Copy(MagicNumber, 0, buffer, offset, MagicNumber.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(SupportedVersion[0]), 0, buffer, offset + MagicNumber.Length + 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(SupportedVersion[1]), 0, buffer, offset + MagicNumber.Length + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(SupportedVersion[2]), 0, buffer, offset + MagicNumber.Length + 8, 4);
        }

        private static uint[] ValidateHeader(byte[] buffer, int offset = 0) {
            var magic = new byte[MagicNumber.Length];
            Array.Copy(buffer, offset, magic, 0, MagicNumber.Length);
            if (!magic.SequenceEqual(MagicNumber)) {
                throw new NetworkConnectError.Handshake(
                    new InitProtocolError<ProtocolsError>.WrongMagicNumber(magic));
            }

            uint v0 = BitConverter.ToUInt32(buffer, offset + MagicNumber.Length + 0);
            uint v1 = BitConverter.ToUInt32(buffer, offset + MagicNumber.Length + 4);
            uint v2 = BitConverter.ToUInt32(buffer, offset + MagicNumber.Length + 8);
            var version = new[] { v0, v1, v2 };
            if (v0 != SupportedVersion[0] || v1 != SupportedVersion[1]) {
                throw new NetworkConnectError.Handshake(
                    new InitProtocolError<ProtocolsError>.WrongVersion(version));
            }
            return version;
        }

        private static void WriteInit(byte[] buffer, Pid pid, Guid secret, HandshakeFeatures features, int offset = 0) {
            var pidBytes = pid.ToByteArray();
            Array.Copy(pidBytes, 0, buffer, offset, 16);
            var secretBytes = secret.ToByteArray();
            Array.Copy(secretBytes, 0, buffer, offset + 16, 16);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)features), 0, buffer, offset + 32, 4);
        }

        private static (Pid pid, Guid secret, HandshakeFeatures features) ValidateInit(byte[] buffer, int offset = 0) {
            var pidBytes = new byte[16];
            Array.Copy(buffer, offset, pidBytes, 0, 16);
            var secretBytes = new byte[16];
            Array.Copy(buffer, offset + 16, secretBytes, 0, 16);
            var pid = new Pid(new Guid(pidBytes));
            var secret = new Guid(secretBytes);
            uint flags = BitConverter.ToUInt32(buffer, offset + 32);
            var features = (HandshakeFeatures)flags;
            return (pid, secret, features);
        }

        private static async Task ReadExactAsync(Stream stream, byte[] buffer, int len, CancellationToken token) {
            int read = 0;
            while (read < len) {
                int r = await stream.ReadAsync(buffer.AsMemory(read, len - read), token);
                if (r == 0) throw new NetworkConnectError.Handshake(new InitProtocolError<ProtocolsError>.NotHandshake());
                read += r;
            }
        }

        private static void WriteHandshake(byte[] buffer, Pid pid, Guid secret, HandshakeFeatures features) {
            WriteHeader(buffer, 0);
            WriteInit(buffer, pid, secret, features, HeaderSize);
        }

        private static (Pid pid, Guid secret, HandshakeFeatures features, uint[] version) ValidateHandshake(byte[] buffer) {
            var version = ValidateHeader(buffer, 0);
            var (pid, secret, feat) = ValidateInit(buffer, HeaderSize);
            return (pid, secret, feat, version);
        }

        internal static byte[] GetBytes(Pid pid, Guid secret, HandshakeFeatures features = HandshakeFeatures.None) {
            var buffer = new byte[HandshakeSize];
            WriteHandshake(buffer, pid, secret, features);
            return buffer;
        }

        internal static bool TryParse(byte[] data, out Pid pid, out Guid secret, out HandshakeFeatures features, out uint[] version) {
            pid = default;
            secret = default;
            features = HandshakeFeatures.None;
            version = Array.Empty<uint>();
            if (data.Length != HandshakeSize) return false;
            try {
                (pid, secret, features, version) = ValidateHandshake(data);
                return true;
            } catch {
                return false;
            }
        }
    }
}
