using System.Threading.Tasks;
using VelorenPort.Network;

namespace Network.Tests;

public class MpscConnectionTests
{
    [Fact]
    public async Task ConnectSendAndDisconnect()
    {
        var netA = new Network(Pid.NewPid());
        var netB = new Network(Pid.NewPid());
        const ulong channelId = 1234;

        await netB.ListenAsync(new ListenAddr.Mpsc(channelId));
        var paTask = netA.ConnectAsync(new ConnectAddr.Mpsc(channelId));
        var pb = await netB.ConnectedAsync();
        var pa = await paTask;

        Assert.NotNull(pb);
        Assert.NotEmpty(pa.RemoteVersion);
        Assert.True(pa.TryGetStream(new Sid(0), out var sa));
        Assert.True(pb!.TryGetStream(new Sid(0), out var sb));

        var msg = Message.Serialize("ping", new StreamParams(Promises.Ordered));
        await sa.SendAsync(msg);
        var recv = await sb.RecvAsync();
        Assert.Equal("ping", recv?.Deserialize<string>());

        await netA.DisconnectAsync(pa.Id);
        Assert.False(netA.TryGetParticipant(pa.Id, out _));

        await netA.ShutdownAsync();
        await netB.ShutdownAsync();
    }
}
