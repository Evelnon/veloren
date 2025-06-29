using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VelorenPort.Server;
using VelorenPort.Server.Sys;

namespace Server.Tests;

public class QueryClientTests {
    static int FreePort() {
        var l = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int p = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return p;
    }

    [Fact]
    public async Task ClientFetchesServerInfo() {
        int port = FreePort();
        var info = new ServerInfo(1, 2, 0, 10, BattleMode.PvE);
        var server = new QueryServer(new IPEndPoint(IPAddress.Loopback, port), info, 10);
        using var cts = new CancellationTokenSource();
        var task = server.RunAsync(cts.Token);

        var client = new QueryClient(new IPEndPoint(IPAddress.Loopback, port));
        var (resp, _) = await client.ServerInfoAsync();
        Assert.Equal(info.PlayerCap, resp.PlayerCap);
        Assert.Equal(info.BattleMode, resp.BattleMode);

        cts.Cancel();
        await task;
    }
}
