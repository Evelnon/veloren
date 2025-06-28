using System;
using VelorenPort.Server.Sys;
using VelorenPort.Server.Settings;

namespace Server.Tests;

public class ServerInfoTests {
    [Fact]
    public void Broadcaster_Sends_Every_Sixty_Ticks() {
        int calls = 0;
        var broadcaster = new ServerInfoBroadcaster(_ => calls++);
        var settings = new Settings();
        for (ulong tick = 0; tick < 120; tick++)
            broadcaster.Update(tick, settings, 0);
        Assert.Equal(2, calls);
    }
}
