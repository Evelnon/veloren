using System;
using System.Net;
using System.Threading.Tasks;
using VelorenPort.Network;
using Xunit;
using Xunit.Sdk;

namespace Network.Tests;

public class QuicOptionsIntegrationTests
{
    [Fact]
    public async Task ConnectsWithZeroRttEnabled()
    {
        if (!QuicFeatureSupport.SupportsZeroRtt)
            throw new SkipException("Rust server lacks 0-RTT support");
        var cfg = new QuicClientConfig
        {
            EnableZeroRtt = true,
            AllowSessionResumption = true,
            MaxEarlyData = 64,
            IdleTimeout = TimeSpan.FromSeconds(5)
        };
        var (server, net) = await RustServerHarness.StartAndConnectQuicAsync(cfg);
        var p = await net.ConnectedAsync();
        Assert.NotNull(p);
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }

    [Fact]
    public async Task ConnectsWithMigrationEnabled()
    {
        if (!QuicFeatureSupport.SupportsMigration)
            throw new SkipException("Rust server lacks connection migration support");
        var cfg = new QuicClientConfig
        {
            EnableConnectionMigration = true,
            IdleTimeout = TimeSpan.FromSeconds(5)
        };
        var (server, net) = await RustServerHarness.StartAndConnectQuicAsync(cfg);
        var p = await net.ConnectedAsync();
        Assert.NotNull(p);
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }
}
