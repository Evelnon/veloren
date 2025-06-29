using System;
using System.Reflection;
using System.Threading.Tasks;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Network;
using VelorenPort.Server;
using VelorenPort.Server.Sys;
using VelorenPort.Server.Events;
using VelorenPort.NativeMath;
using System.Collections.Generic;

namespace Server.Tests;

public class ChatSystemAutoModTests
{
    private static Client CreateClient()
    {
        var participant = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(1), Guid.NewGuid(), null, null, null })!;
        return (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { participant })!;
    }

    [Fact]
    public void Update_BlocksBannedWords()
    {
        var (cache, exporter) = Chat.ChatCache.Create(TimeSpan.FromSeconds(1));
        var events = new EventManager();
        var client = CreateClient();
        using (var emitter = events.GetChatEmitter())
        {
            var msg = new UnresolvedChatMsg(
                new ChatType<Group>.Say<Group>(client.Uid),
                new Content.Plain("badword"));
            emitter.Emit(new ChatEvent(msg, true));
        }
        var automod = new AutoMod(new ModerationSettings { Automod = true }, new Censor(new[] { "badword" }));
        ChatSystem.Update(events, exporter, automod, new[] { client });
        Task.Delay(50).Wait();
        Assert.Empty(cache.Messages);
    }

    [Fact]
    public void Update_AllowsCleanMessage()
    {
        var (cache, exporter) = Chat.ChatCache.Create(TimeSpan.FromSeconds(1));
        var events = new EventManager();
        var client = CreateClient();
        using (var emitter = events.GetChatEmitter())
        {
            var msg = new UnresolvedChatMsg(
                new ChatType<Group>.Say<Group>(client.Uid),
                new Content.Plain("hello"));
            emitter.Emit(new ChatEvent(msg, true));
        }
        var automod = new AutoMod(new ModerationSettings { Automod = true }, new Censor(new[] { "badword" }));
        ChatSystem.Update(events, exporter, automod, new[] { client });
        Task.Delay(50).Wait();
        Assert.Single(cache.Messages);
    }
}
