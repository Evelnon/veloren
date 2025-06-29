using System.Net;
using System.Threading.Tasks;
using VelorenPort.Network;

namespace Network.Tests;

public class SocketConnectionTests
{
    [Fact]
    public async Task TcpFeatureNegotiationAndMessaging()
    {
        var netA = new Network(Pid.NewPid());
        var netB = new Network(Pid.NewPid());
        int port = TestUtils.GetFreePort();
        var addr = new IPEndPoint(IPAddress.Loopback, port);

        var featuresA = HandshakeFeatures.Compression | HandshakeFeatures.Encryption;
        var featuresB = HandshakeFeatures.Encryption | HandshakeFeatures.ReliableUdp;

        await netA.ListenAsync(new ListenAddr.Tcp(addr), featuresA);
        var connectTask = netB.ConnectAsync(new ConnectAddr.Tcp(addr), featuresB);
        var pb = await netA.ConnectedAsync();
        var pa = await connectTask;

        Assert.NotNull(pb);
        Assert.Equal(HandshakeFeatures.Encryption, pa.Features);
        Assert.Equal(pa.Features, pb!.Features);
        Assert.NotEmpty(pa.RemoteVersion);

        var sa = await pa.OpenStreamAsync(pa.NextSid(), new StreamParams(Promises.Ordered));
        var sb = await pb.OpenedAsync();
        await sa.SendAsync("tcp");
        var recv = await sb.RecvAsync();
        Assert.Equal("tcp", recv?.Deserialize<string>());

        await netA.ShutdownAsync();
        await netB.ShutdownAsync();
    }

    [Fact]
    public async Task UdpFeatureNegotiationAndMessaging()
    {
        var netA = new Network(Pid.NewPid());
        var netB = new Network(Pid.NewPid());
        int port = TestUtils.GetFreePort();
        var addr = new IPEndPoint(IPAddress.Loopback, port);

        var featuresA = HandshakeFeatures.ReliableUdp | HandshakeFeatures.Compression;
        var featuresB = HandshakeFeatures.ReliableUdp;

        await netA.ListenAsync(new ListenAddr.Udp(addr), featuresA);
        var connectTask = netB.ConnectAsync(new ConnectAddr.Udp(addr), featuresB);
        var pb = await netA.ConnectedAsync();
        var pa = await connectTask;

        Assert.NotNull(pb);
        Assert.Equal(HandshakeFeatures.ReliableUdp, pa.Features);
        Assert.Equal(pa.Features, pb!.Features);
        Assert.NotEmpty(pa.RemoteVersion);

        var sa = await pa.OpenStreamAsync(pa.NextSid(), new StreamParams(Promises.Ordered | Promises.GuaranteedDelivery));
        var sb = await pb.OpenedAsync();
        await sa.SendAsync("udp");
        var recv = await sb.RecvAsync();
        Assert.Equal("udp", recv?.Deserialize<string>());

        await netA.ShutdownAsync();
        await netB.ShutdownAsync();
    }

    [Fact]
    public async Task PrioritySchedulingMpsc()
    {
        var netA = new Network(Pid.NewPid());
        var netB = new Network(Pid.NewPid());
        const ulong channelId = 5555;

        await netB.ListenAsync(new ListenAddr.Mpsc(channelId));
        var paTask = netA.ConnectAsync(new ConnectAddr.Mpsc(channelId));
        var pb = await netB.ConnectedAsync();
        var pa = await paTask;

        var sa = await pa.OpenStreamAsync(pa.NextSid(), new StreamParams(Promises.Ordered));
        var sb = await pb.OpenedAsync();

        await sa.SendAsync("low1", prio: 3);
        await sa.SendAsync("high", prio: 0);
        await sa.SendAsync("low2", prio: 3);

        var r1 = await sb.RecvAsync<string>();
        var r2 = await sb.RecvAsync<string>();
        var r3 = await sb.RecvAsync<string>();

        Assert.Equal("high", r1);
        Assert.Equal("low1", r2);
        Assert.Equal("low2", r3);

        await netA.ShutdownAsync();
        await netB.ShutdownAsync();
    }
}
