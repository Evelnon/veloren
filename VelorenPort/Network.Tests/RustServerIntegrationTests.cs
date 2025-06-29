using System.Threading.Tasks;

namespace Network.Tests;

public class RustServerIntegrationTests
{
    [Fact]
    public async Task ConnectsToRustServerIfAvailable()
    {
        var net = await RustServerHarness.ConnectOrSkipAsync();
        await net.ShutdownAsync();
    }

    [Fact]
    public async Task ConnectsToRustServerViaQuicIfAvailable()
    {
        var net = await RustServerHarness.ConnectQuicOrSkipAsync();
        await net.ShutdownAsync();
    }
}
