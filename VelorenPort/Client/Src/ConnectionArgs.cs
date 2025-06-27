using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using VelorenPort.CoreEngine;
using VelorenPort.Network;

namespace VelorenPort.Client {
    /// <summary>
    /// Represents connection parameters for the client. Mirrors the
    /// <c>ConnectionArgs</c> enum from the Rust implementation.
    /// </summary>
    [Serializable]
    public abstract record ConnectionArgs {
        public sealed record Quic(string Hostname, bool PreferIpv6, bool ValidateTls) : ConnectionArgs;
        public sealed record Tcp(string Hostname, bool PreferIpv6) : ConnectionArgs;
        public sealed record Srv(string Hostname, bool PreferIpv6, bool ValidateTls, bool UseQuic) : ConnectionArgs;
        public sealed record Mpsc(ulong ChannelId) : ConnectionArgs;
    }

    /// <summary>
    /// Helper methods for resolving hostnames and attempting connections.
    /// </summary>
    public static class ConnectionUtil {
        private const int DefaultPort = 14004;

        /// <summary>Resolve an address string to a set of socket endpoints.</summary>
        public static async Task<List<IPEndPoint>> ResolveAsync(string address, bool preferIpv6) {
            if (address is null)
                throw new ArgumentNullException(nameof(address));

            ParseAddress(address, out var host, out var port);
            var addresses = await Dns.GetHostAddressesAsync(host);
            var ordered = OrderAddresses(addresses, preferIpv6);
            return ordered.Select(ip => new IPEndPoint(ip, port)).ToList();
        }

        /// <summary>Try connecting using each resolved address.</summary>
        public static async Task<Result<Participant, Error>> TryConnectAsync(
            Network.Network network,
            string address,
            int? overridePort,
            bool preferIpv6,
            Func<IPEndPoint, ConnectAddr> map)
        {
            if (network is null)
                throw new ArgumentNullException(nameof(network));
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            var endpoints = await ResolveAsync(address, preferIpv6);
            Error? lastError = null;
            foreach (var ep in endpoints) {
                var finalEp = overridePort.HasValue ? new IPEndPoint(ep.Address, overridePort.Value) : ep;
                try {
                    var participant = await network.ConnectAsync(map(finalEp));
                    return Result<Participant, Error>.Ok(participant);
                } catch (NetworkError ne) {
                    lastError = new Error.NetworkErr(ne);
                } catch (Exception ex) {
                    lastError = new Error.Other(ex.Message);
                }
            }
            return Result<Participant, Error>.Err(lastError ?? new Error.Other("No Ip Addr provided"));
        }

        private static void ParseAddress(string address, out string host, out int port) {
            host = address;
            port = DefaultPort;

            if (address.StartsWith("[") && address.Contains("]")) {
                var end = address.IndexOf(']');
                host = address.Substring(1, end - 1);
                var rest = address[(end + 1)..];
                if (rest.StartsWith(":"))
                    int.TryParse(rest[1..], out port);
                return;
            }

            var lastColon = address.LastIndexOf(':');
            if (lastColon > 0 && int.TryParse(address[(lastColon + 1)..], out var parsed)) {
                host = address[..lastColon];
                port = parsed;
            }
        }

        private static IEnumerable<IPAddress> OrderAddresses(IEnumerable<IPAddress> addrs, bool preferIpv6) {
            var first = new List<IPAddress>();
            var second = new List<IPAddress>();
            foreach (var ip in addrs) {
                if ((ip.AddressFamily == AddressFamily.InterNetworkV6) == preferIpv6)
                    first.Add(ip);
                else
                    second.Add(ip);
            }
            return first.Concat(second);
        }
    }
}
