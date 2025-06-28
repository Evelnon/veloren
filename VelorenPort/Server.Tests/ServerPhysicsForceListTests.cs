using System;
using System.IO;
using VelorenPort.Server.Settings;

namespace Server.Tests;

public class ServerPhysicsForceListTests {
    [Fact]
    public void Force_AddsRecord() {
        var list = new ServerPhysicsForceList();
        var id = Guid.NewGuid();
        list.Force(id, new ServerPhysicsForceList.ServerPhysicsForceRecord(null, "cheat"));
        Assert.True(list.Contains(id));
    }

    [Fact]
    public void Save_Load_RoundTrip() {
        var tmp = Path.GetTempFileName();
        var list = new ServerPhysicsForceList();
        var id = Guid.NewGuid();
        list.Force(id, new ServerPhysicsForceList.ServerPhysicsForceRecord(Guid.Empty, "test"));
        list.Save(tmp);
        var loaded = ServerPhysicsForceList.Load(tmp);
        Assert.True(loaded.Contains(id));
        File.Delete(tmp);
    }
}
