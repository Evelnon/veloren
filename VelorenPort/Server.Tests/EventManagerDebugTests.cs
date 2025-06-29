using System;
using VelorenPort.Server.Events;
using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace Server.Tests;

public class EventManagerDebugTests
{
    [Fact]
    public void DebugCheckThrowsOnUnconsumedEvents()
    {
        var manager = new EventManager();
        using (var emitter = manager.GetEmitter<TeleportToPositionEvent>())
        {
            emitter.Emit(new TeleportToPositionEvent(new Uid(1), new float3(0,0,0)));
        }
#if DEBUG
        Assert.Throws<InvalidOperationException>(() => manager.DebugCheckAllConsumed());
#endif
    }

    [Fact]
    public void DebugCheckPassesWhenEventsConsumedOnce()
    {
        var manager = new EventManager();
        using (var emitter = manager.GetEmitter<TeleportToPositionEvent>())
        {
            emitter.Emit(new TeleportToPositionEvent(new Uid(1), float3.zero));
        }
        _ = manager.Drain<TeleportToPositionEvent>();
#if DEBUG
        manager.DebugCheckAllConsumed();
#endif
    }

    [Fact]
    public void DebugCheckThrowsOnMultipleConsumers()
    {
        var manager = new EventManager();
        using (var emitter = manager.GetEmitter<TeleportToPositionEvent>())
        {
            emitter.Emit(new TeleportToPositionEvent(new Uid(1), float3.zero));
        }
        _ = manager.Drain<TeleportToPositionEvent>();
        _ = manager.Drain<TeleportToPositionEvent>();
#if DEBUG
        Assert.Throws<InvalidOperationException>(() => manager.DebugCheckAllConsumed());
#endif
    }
}
