using System.Threading.Tasks;
using VelorenPort.Network;
using Xunit;

namespace Network.Tests;

public class RustServerProtocolTests
{
    [Fact]
    public async Task HandshakeNegotiation()
    {
        var (server, net) = await RustServerHarness.StartAndConnectAsync();
        Assert.NotEmpty(net.LocalPid.ToByteArray());
        var participant = await net.ConnectedAsync();
        Assert.NotNull(participant);
        Assert.NotEmpty(participant!.RemoteVersion);
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }

    [Fact]
    public async Task StreamReliability()
    {
        var (server, net) = await RustServerHarness.StartAndConnectAsync();
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
        var (server, net) = await RustServerHarness.StartAndConnectAsync(useQuic: true);
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }
}
