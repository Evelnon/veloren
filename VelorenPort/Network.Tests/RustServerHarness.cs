using System;
using System.Threading.Tasks;
using System.Net;
using Xunit;
using VelorenPort.Network;

namespace Network.Tests;

using System.Diagnostics;

internal static class RustServerHarness
{
    public static Process StartServer()
    {
        var psi = new ProcessStartInfo("cargo", "test-server -- --non-interactive --no-auth")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = TestUtils.GetRepoRoot(),
        };
        var proc = Process.Start(psi)!;
        Task.Delay(5000).Wait();
        return proc;
    }

    public static async Task StopServerAsync(Process proc)
    {
        if (!proc.HasExited)
        {
            proc.Kill();
            await proc.WaitForExitAsync();
        }
    }
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

    public static async Task<Network> ConnectQuicOrSkipAsync()
    {
        string host = Environment.GetEnvironmentVariable("RUST_SERVER_ADDR") ?? "127.0.0.1";
        int port = 14004;
        if (int.TryParse(Environment.GetEnvironmentVariable("RUST_SERVER_PORT"), out var envPort))
            port = envPort;

        var addr = new IPEndPoint(IPAddress.Parse(host), port);
        var cfg = new QuicClientConfig { EnableZeroRtt = true, EnableConnectionMigration = true, AllowSessionResumption = true };
        var net = new Network(Pid.NewPid());
        try
        {
            await net.ConnectAsync(new ConnectAddr.Quic(addr, cfg, "quic"));
        }
        catch
        {
            throw new SkipException($"Rust server (QUIC) not running at {addr}");
        }
        return net;
    }
}
