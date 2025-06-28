using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Implements a minimal handshake verifying magic number and network version.
    /// Mirrors part of the Rust handshake.rs logic.
    /// </summary>
    internal static class Handshake {
        internal static readonly byte[] MagicNumber = Encoding.ASCII.GetBytes("VELOREN");
        internal static readonly uint[] NetworkVersion = { 0u, 6u, 0u };

        private static int HandshakeSize => MagicNumber.Length + NetworkVersion.Length * 4 + 16 + 16;

        public static async Task<(Pid remotePid, Guid remoteSecret)> PerformAsync(
            Stream stream,
            bool initiator,
            Pid localPid,
            Guid localSecret,
            CancellationToken token = default)
        {
            var buffer = new byte[HandshakeSize];

            WriteHandshake(buffer, localPid, localSecret);

            if (initiator) {
                await stream.WriteAsync(buffer, 0, buffer.Length, token);
                await stream.FlushAsync(token);
            }

            await ReadExactAsync(stream, buffer, buffer.Length, token);
            var (pid, secret) = ValidateHandshake(buffer);

            if (!initiator) {
                WriteHandshake(buffer, localPid, localSecret);
                await stream.WriteAsync(buffer, 0, buffer.Length, token);
                await stream.FlushAsync(token);
            }

            return (pid, secret);
        }

        private static void WriteHandshake(byte[] buffer, Pid pid, Guid secret) {
            Array.Copy(MagicNumber, 0, buffer, 0, MagicNumber.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(NetworkVersion[0]), 0, buffer, MagicNumber.Length + 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(NetworkVersion[1]), 0, buffer, MagicNumber.Length + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(NetworkVersion[2]), 0, buffer, MagicNumber.Length + 8, 4);
            var pidBytes = pid.ToByteArray();
            Array.Copy(pidBytes, 0, buffer, MagicNumber.Length + 12, 16);
            var secretBytes = secret.ToByteArray();
            Array.Copy(secretBytes, 0, buffer, MagicNumber.Length + 28, 16);
        }

        private static (Pid pid, Guid secret) ValidateHandshake(byte[] buffer) {
            var magic = new byte[MagicNumber.Length];
            Array.Copy(buffer, 0, magic, 0, MagicNumber.Length);
            if (!magic.SequenceEqual(MagicNumber)) {
                throw new NetworkConnectError.Handshake(
                    new InitProtocolError<ProtocolsError>.WrongMagicNumber(magic));
            }

            uint v0 = BitConverter.ToUInt32(buffer, MagicNumber.Length + 0);
            uint v1 = BitConverter.ToUInt32(buffer, MagicNumber.Length + 4);
            uint v2 = BitConverter.ToUInt32(buffer, MagicNumber.Length + 8);
            var version = new[] { v0, v1, v2 };
            if (v0 != NetworkVersion[0] || v1 != NetworkVersion[1]) {
                throw new NetworkConnectError.Handshake(
                    new InitProtocolError<ProtocolsError>.WrongVersion(version));
            }
            var pidBytes = new byte[16];
            Array.Copy(buffer, MagicNumber.Length + 12, pidBytes, 0, 16);
            var secretBytes = new byte[16];
            Array.Copy(buffer, MagicNumber.Length + 28, secretBytes, 0, 16);
            var pid = new Pid(new Guid(pidBytes));
            var secret = new Guid(secretBytes);
            return (pid, secret);
        }

        private static async Task ReadExactAsync(Stream stream, byte[] buffer, int len, CancellationToken token) {
            int read = 0;
            while (read < len) {
                int r = await stream.ReadAsync(buffer.AsMemory(read, len - read), token);
                if (r == 0) throw new NetworkConnectError.Handshake(new InitProtocolError<ProtocolsError>.NotHandshake());
                read += r;
            }
        }

        internal static byte[] GetBytes(Pid pid, Guid secret) {
            var buffer = new byte[HandshakeSize];
            WriteHandshake(buffer, pid, secret);
            return buffer;
        }

        internal static bool TryParse(byte[] data, out Pid pid, out Guid secret) {
            pid = default;
            secret = default;
            if (data.Length != HandshakeSize) return false;
            try {
                (pid, secret) = ValidateHandshake(data);
                return true;
            } catch {
                return false;
            }
        }
    }
}
