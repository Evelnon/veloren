using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class EventBusTests
{
    [Fact]
    public void EmitNow_AddsEventImmediately()
    {
        var bus = new EventBus<int>();
        bus.EmitNow(5);
        var events = bus.RecvAll();
        Assert.Single(events, 5);
    }

    [Fact]
    public void Emitter_BatchesEventsUntilDispose()
    {
        var bus = new EventBus<string>();
        using (var emitter = bus.GetEmitter())
        {
            emitter.Emit("a");
            emitter.EmitMany(new[] { "b", "c" });
        }
        var events = bus.RecvAll();
        Assert.Equal(new[] { "a", "b", "c" }, events);
    }

    [Fact]
    public void RecvAllMut_SkipsLockingAndClearsQueue()
    {
        var bus = new EventBus<int>();
        bus.EmitNow(1);
        bus.EmitNow(2);
        var list = bus.RecvAllMut();
        Assert.Equal(new[] {1,2}, list);
        Assert.Empty(bus.RecvAll());
    }
}
