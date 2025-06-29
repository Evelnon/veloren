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
        private static int InitSizeLegacy => 16 + 16;
        private static int HandshakeSize => HeaderSize + InitSize;
        private static int HandshakeSizeLegacy => HeaderSize + InitSizeLegacy;

        private enum HandshakeStep
        {
            SendHandshake,
            ReceiveHandshake,
            SendInit,
            ReceiveInit,
            SendConfirm,
            ReceiveConfirm,
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
            WriteHeader(header);

            uint[] version = Array.Empty<uint>();
            Pid pid = default;
            Guid secret = default;
            HandshakeFeatures remoteFlags = HandshakeFeatures.None;
            bool legacy = false;

            var step = initiator ? HandshakeStep.SendHandshake : HandshakeStep.ReceiveHandshake;
            byte[] initSend = Array.Empty<byte>();
            byte[] initRecv = Array.Empty<byte>();
            var confirm = new byte[1] { 1 };

            while (step != HandshakeStep.Complete)
            {
                switch (step)
                {
                    case HandshakeStep.SendHandshake:
                        await stream.WriteAsync(header, 0, header.Length, token);
                        await stream.FlushAsync(token);
                        step = HandshakeStep.ReceiveHandshake;
                        break;
                    case HandshakeStep.ReceiveHandshake:
                        await ReadExactAsync(stream, header, header.Length, token);
                        version = ValidateHeader(header);
                        legacy = version[0] == SupportedVersion[0] && version[1] < SupportedVersion[1];
                        int initSize = legacy ? InitSizeLegacy : InitSize;
                        initSend = new byte[initSize];
                        initRecv = new byte[initSize];
                        if (!initiator)
                        {
                            WriteInit(initSend, localPid, localSecret, localFeatures, 0, legacy);
                            WriteHeader(header);
                            await stream.WriteAsync(header, 0, header.Length, token);
                            await stream.FlushAsync(token);
                            step = HandshakeStep.ReceiveInit;
                        }
                        else
                        {
                            step = HandshakeStep.SendInit;
                        }
                        break;
                    case HandshakeStep.SendInit:
                        WriteInit(initSend, localPid, localSecret, localFeatures, 0, legacy);
                        await stream.WriteAsync(initSend, 0, initSend.Length, token);
                        await stream.FlushAsync(token);
                        step = HandshakeStep.ReceiveInit;
                        break;
                    case HandshakeStep.ReceiveInit:
                        await ReadExactAsync(stream, initRecv, initRecv.Length, token);
                        (pid, secret, remoteFlags) = ValidateInit(initRecv, 0, legacy);
                        if (!initiator)
                        {
                            WriteInit(initSend, localPid, localSecret, localFeatures, 0, legacy);
                            await stream.WriteAsync(initSend, 0, initSend.Length, token);
                            await stream.FlushAsync(token);
                            step = legacy ? HandshakeStep.Complete : HandshakeStep.ReceiveConfirm;
                        }
                        else
                        {
                            step = legacy ? HandshakeStep.Complete : HandshakeStep.SendConfirm;
                        }
                        break;
                    case HandshakeStep.SendConfirm:
                        await stream.WriteAsync(confirm, 0, 1, token);
                        await stream.FlushAsync(token);
                        step = HandshakeStep.ReceiveConfirm;
                        break;
                    case HandshakeStep.ReceiveConfirm:
                        await ReadExactAsync(stream, confirm, 1, token);
                        if (!legacy && confirm[0] != 1)
                            throw new NetworkConnectError.Handshake(new InitProtocolError<ProtocolsError>.NotHandshake());
                        if (!initiator)
                            step = HandshakeStep.SendConfirm;
                        else
                            step = HandshakeStep.Complete;
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
            if (v0 != SupportedVersion[0] || v1 > SupportedVersion[1]) {
                throw new NetworkConnectError.Handshake(
                    new InitProtocolError<ProtocolsError>.WrongVersion(version));
            }
            return version;
        }

        private static void WriteInit(byte[] buffer, Pid pid, Guid secret, HandshakeFeatures features, int offset = 0, bool legacy = false) {
            var pidBytes = pid.ToByteArray();
            Array.Copy(pidBytes, 0, buffer, offset, 16);
            var secretBytes = secret.ToByteArray();
            Array.Copy(secretBytes, 0, buffer, offset + 16, 16);
            if (!legacy)
                Buffer.BlockCopy(BitConverter.GetBytes((uint)features), 0, buffer, offset + 32, 4);
        }

        private static (Pid pid, Guid secret, HandshakeFeatures features) ValidateInit(byte[] buffer, int offset = 0, bool legacy = false) {
            var pidBytes = new byte[16];
            Array.Copy(buffer, offset, pidBytes, 0, 16);
            var secretBytes = new byte[16];
            Array.Copy(buffer, offset + 16, secretBytes, 0, 16);
            var pid = new Pid(new Guid(pidBytes));
            var secret = new Guid(secretBytes);
            HandshakeFeatures features = HandshakeFeatures.None;
            if (!legacy) {
                uint flags = BitConverter.ToUInt32(buffer, offset + 32);
                features = (HandshakeFeatures)flags;
            }
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

        private static void WriteHandshake(byte[] buffer, Pid pid, Guid secret, HandshakeFeatures features, bool legacy) {
            WriteHeader(buffer, 0);
            WriteInit(buffer, pid, secret, features, HeaderSize, legacy);
        }

        private static (Pid pid, Guid secret, HandshakeFeatures features, uint[] version) ValidateHandshake(byte[] buffer, bool legacy) {
            var version = ValidateHeader(buffer, 0);
            var (pid, secret, feat) = ValidateInit(buffer, HeaderSize, legacy);
            return (pid, secret, feat, version);
        }

        internal static byte[] GetBytes(Pid pid, Guid secret, HandshakeFeatures features = HandshakeFeatures.None) {
            var buffer = new byte[HandshakeSize];
            WriteHandshake(buffer, pid, secret, features, false);
            return buffer;
        }

        internal static bool TryParse(byte[] data, out Pid pid, out Guid secret, out HandshakeFeatures features, out uint[] version) {
            pid = default;
            secret = default;
            features = HandshakeFeatures.None;
            version = Array.Empty<uint>();
            if (data.Length != HandshakeSize && data.Length != HandshakeSizeLegacy) return false;
            try {
                bool legacy = data.Length == HandshakeSizeLegacy;
                (pid, secret, features, version) = ValidateHandshake(data, legacy);
                return true;
            } catch {
                return false;
            }
        }
    }
}
