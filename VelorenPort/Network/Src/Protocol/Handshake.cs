using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VelorenPort.Network;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Wrapper over the higher level Handshake implementation for protocol code.
    /// </summary>
    public static class Handshake {
        public static Task<(Pid pid, Guid secret, HandshakeFeatures features, uint[] version, Sid offset)> PerformAsync(
            Stream stream,
            bool initiator,
            Pid localPid,
            Guid localSecret,
            HandshakeFeatures localFeatures = HandshakeFeatures.None,
            CancellationToken token = default)
        {
            return Network.Handshake.PerformAsync(stream, initiator, localPid, localSecret, localFeatures, token);
        }

        public static byte[] GetBytes(Pid pid, Guid secret, HandshakeFeatures features = HandshakeFeatures.None)
            => Network.Handshake.GetBytes(pid, secret, features);

        public static bool TryParse(byte[] data, out Pid pid, out Guid secret, out HandshakeFeatures features, out uint[] version)
            => Network.Handshake.TryParse(data, out pid, out secret, out features, out version);
    }
}
