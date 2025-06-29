using VelorenPort.Server.Events;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server;
using VelorenPort.NativeMath;

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

    [Fact]
    public void GenericEmitter_WorksForCustomEvent()
    {
        var manager = new EventManager();
        using (var emitter = manager.GetEmitter<CreateItemDropEvent>())
        {
            emitter.Emit(new CreateItemDropEvent(new float3(1, 2, 3), "wood", 1));
        }
        var evs = manager.Drain<CreateItemDropEvent>();
        Assert.Single(evs);
        Assert.Equal(new float3(1, 2, 3), evs[0].Position);
        Assert.Equal("wood", evs[0].Item);
        Assert.Equal((uint)1, evs[0].Amount);
    }
}
