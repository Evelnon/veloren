using System;
using System.Net;
using System.Threading.Tasks;
using VelorenPort.Network;
using Xunit;

namespace Network.Tests;

public class QuicOptionsIntegrationTests
{
    [Fact]
    public async Task ConnectsWithZeroRttEnabled()
    {
        using var server = RustServerHarness.StartServer();
        var cfg = new QuicClientConfig
        {
            EnableZeroRtt = true,
            AllowSessionResumption = true,
            MaxEarlyData = 64,
            IdleTimeout = TimeSpan.FromSeconds(5)
        };
        var net = new Network(Pid.NewPid());
        await net.ConnectAsync(new ConnectAddr.Quic(new IPEndPoint(IPAddress.Loopback, 14004), cfg, "quic"));
        var p = await net.ConnectedAsync();
        Assert.NotNull(p);
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }

    [Fact]
    public async Task ConnectsWithMigrationEnabled()
    {
        using var server = RustServerHarness.StartServer();
        var cfg = new QuicClientConfig
        {
            EnableConnectionMigration = true,
            IdleTimeout = TimeSpan.FromSeconds(5)
        };
        var net = new Network(Pid.NewPid());
        await net.ConnectAsync(new ConnectAddr.Quic(new IPEndPoint(IPAddress.Loopback, 14004), cfg, "quic"));
        var p = await net.ConnectedAsync();
        Assert.NotNull(p);
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }
}
