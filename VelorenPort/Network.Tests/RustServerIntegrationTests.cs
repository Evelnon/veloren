using System.Threading.Tasks;

namespace Network.Tests;

public class RustServerIntegrationTests
{
    [Fact]
    public async Task ConnectsToRustServerIfAvailable()
    {
        var (server, net) = await RustServerHarness.StartAndConnectAsync();
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }

    [Fact]
    public async Task ConnectsToRustServerViaQuicIfAvailable()
    {
        var (server, net) = await RustServerHarness.StartAndConnectAsync(useQuic: true);
        await net.ShutdownAsync();
        await RustServerHarness.StopServerAsync(server);
    }
}
