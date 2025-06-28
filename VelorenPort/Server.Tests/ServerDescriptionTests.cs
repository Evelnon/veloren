using System.IO;
using VelorenPort.Server.Settings;

namespace Server.Tests;

public class ServerDescriptionTests {
    [Fact]
    public void Load_NoFile_ReturnsDefault() {
        var desc = ServerDescriptions.Load("nonexistent.json");
        Assert.NotNull(desc.Get("en"));
    }

    [Fact]
    public void Save_And_Load_RoundTrip() {
        var tmp = Path.GetTempFileName();
        var descs = new ServerDescriptions();
        descs.Descriptions["en"] = new ServerDescription { Motd = "hi", Rules = "r" };
        descs.Save(tmp);
        var loaded = ServerDescriptions.Load(tmp);
        Assert.Equal("hi", loaded.Get("en").Motd);
        Assert.Equal("r", loaded.GetRules("en"));
        File.Delete(tmp);
    }
}
