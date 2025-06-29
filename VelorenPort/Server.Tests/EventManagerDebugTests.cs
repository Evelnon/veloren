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
}
