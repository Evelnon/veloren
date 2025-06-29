using System.Threading.Tasks;
using VelorenPort.Network;
using Xunit;

namespace Network.Tests;

public class RustServerProtocolTests
{
    [Fact]
    public async Task HandshakeNegotiation()
    {
        using var server = RustServerHarness.StartServer();
        var net = await RustServerHarness.ConnectOrSkipAsync();
        Assert.NotEmpty(net.LocalPid.ToByteArray());
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }

    [Fact]
    public async Task StreamReliability()
    {
        using var server = RustServerHarness.StartServer();
        var net = await RustServerHarness.ConnectOrSkipAsync();
        var p = await net.ConnectedAsync();
        var s = await p!.OpenStreamAsync(p.NextSid(), new StreamParams(Promises.Ordered | Promises.GuaranteedDelivery));
        await s.SendAsync("ping");
        var echo = await p.OpenedAsync();
        var msg = await echo.RecvAsync<(string, string)>();
        Assert.Equal("ping", msg?.Item2);
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }

    [Fact]
    public async Task QuicConnectionOptions()
    {
        using var server = RustServerHarness.StartServer();
        var net = await RustServerHarness.ConnectQuicOrSkipAsync();
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }
}
