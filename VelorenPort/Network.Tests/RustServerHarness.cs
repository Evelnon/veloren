using System;
using System.Threading.Tasks;
using System.Net;
using Xunit;
using VelorenPort.Network;

namespace Network.Tests;

internal static class RustServerHarness
{
    public static async Task<Network> ConnectOrSkipAsync()
    {
        string host = Environment.GetEnvironmentVariable("RUST_SERVER_ADDR") ?? "127.0.0.1";
        int port = 14004;
        if (int.TryParse(Environment.GetEnvironmentVariable("RUST_SERVER_PORT"), out var envPort))
            port = envPort;

        var addr = new IPEndPoint(IPAddress.Parse(host), port);
        var net = new Network(Pid.NewPid());
        try
        {
            await net.ConnectAsync(new ConnectAddr.Tcp(addr));
        }
        catch
        {
            throw new SkipException($"Rust server not running at {addr}");
        }
        return net;
    }
}
