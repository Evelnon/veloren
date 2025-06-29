using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using VelorenPort.Server;
using VelorenPort.Server.Sys;

namespace Server.Tests;

public class QueryServerTests {
    static int FreePort() {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int p = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return p;
    }

    [Fact]
    public async Task InitAndServerInfoRequests() {
        int port = FreePort();
        var info = new ServerInfo(1, 2, 0, 10, BattleMode.PvE);
        var server = new QueryServer(new IPEndPoint(IPAddress.Loopback, port), info, 10);
        using var cts = new CancellationTokenSource();
        var task = server.RunAsync(cts.Token);

        using var client = new UdpClient();
        var ep = new IPEndPoint(IPAddress.Loopback, port);
        var req = QueryProtocol.SerializeRequest(0, QueryServerRequest.Init);
        await client.SendAsync(req, req.Length, ep);
        var res = await client.ReceiveAsync();
        Assert.True(QueryProtocol.TryParseInitResponse(res.Buffer, out var challenge));

        req = QueryProtocol.SerializeRequest(challenge, QueryServerRequest.ServerInfo);
        await client.SendAsync(req, req.Length, ep);
        res = await client.ReceiveAsync();
        Assert.True(QueryProtocol.TryParseInfoResponse(res.Buffer, out var resp));
        Assert.Equal(info.PlayerCap, resp.PlayerCap);
        Assert.Equal(info.BattleMode, resp.BattleMode);

        cts.Cancel();
        await task;
    }
}
