using VelorenPort.Server.Events;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server;

namespace Server.Tests;

public class EventManagerTests
{
    [Fact]
    public void EmitChatEvent_CollectsEvent()
    {
        var manager = new EventManager();
        using (var emitter = manager.GetChatEmitter())
        {
            var chatType = new ChatType<Group>.World<Group>(new Uid(1));
            var msg = new UnresolvedChatMsg(chatType, new Content.Plain("hi"));
            emitter.Emit(new ChatEvent(msg, true));
        }
        var events = manager.DrainChatEvents();
        Assert.Single(events);
        Assert.Equal("hi", (events[0].Msg.Content as Content.Plain)!.Text);
        Assert.True(events[0].FromClient);
    }
}
