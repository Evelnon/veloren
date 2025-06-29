using System.Threading.Tasks;
using VelorenPort.Server;
using VelorenPort.Network;

namespace Server.Tests;

public class ClientChannelTests
{
    [Fact]
    public async Task ClientInitializesDefaultStreams()
    {
        var netA = new Network.Network(Pid.NewPid());
        var netB = new Network.Network(Pid.NewPid());
        const ulong channelId = 5678;

        await netB.ListenAsync(new ListenAddr.Mpsc(channelId));
        var connectTask = netA.ConnectAsync(new ConnectAddr.Mpsc(channelId));
        var serverParticipant = await netB.ConnectedAsync();
        var clientParticipant = await connectTask;

        var client = await Client.CreateAsync(serverParticipant!);

        // Accept opened streams on client side
        for (int i = 0; i < 6; i++)
            await clientParticipant.OpenedAsync();

        Assert.True(client.TryGetStream(0, out var register));
        Assert.Equal(Promises.Ordered | Promises.Consistency | Promises.Compressed, register.Promises);
        Assert.Equal((byte)3, register.Priority);
        Assert.Equal((ulong)500, register.GuaranteedBandwidth);

        Assert.True(client.TryGetStream(1, out var charScreen));
        Assert.Equal(register.Promises, charScreen.Promises);
        Assert.Equal(register.Priority, charScreen.Priority);

        Assert.True(client.TryGetStream(2, out var inGame));
        Assert.Equal(register.Promises, inGame.Promises);
        Assert.Equal((ulong)100_000, inGame.GuaranteedBandwidth);

        Assert.True(client.TryGetStream(3, out var general));
        Assert.Equal(register.Promises, general.Promises);

        Assert.True(client.TryGetStream(4, out var ping));
        Assert.Equal(Promises.Ordered | Promises.Consistency, ping.Promises);
        Assert.Equal((byte)2, ping.Priority);

        Assert.True(client.TryGetStream(5, out var terrain));
        Assert.Equal(Promises.Ordered | Promises.Consistency, terrain.Promises);
        Assert.Equal((byte)4, terrain.Priority);
        Assert.Equal((ulong)20_000, terrain.GuaranteedBandwidth);

        await netA.ShutdownAsync();
        await netB.ShutdownAsync();
    }
}
