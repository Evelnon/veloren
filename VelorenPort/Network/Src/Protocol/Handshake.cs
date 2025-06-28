using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Wrapper over the higher level Handshake implementation for protocol code.
    /// </summary>
    public static class Handshake {
        public static Task<(Pid pid, Guid secret)> PerformAsync(
            Stream stream,
            bool initiator,
            Pid localPid,
            Guid localSecret,
            CancellationToken token = default)
        {
            return Network.Handshake.PerformAsync(stream, initiator, localPid, localSecret, token);
        }

        public static byte[] GetBytes(Pid pid, Guid secret) => Network.Handshake.GetBytes(pid, secret);

        public static bool TryParse(byte[] data, out Pid pid, out Guid secret) => Network.Handshake.TryParse(data, out pid, out secret);
    }
}
