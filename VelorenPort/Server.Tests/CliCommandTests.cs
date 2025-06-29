using System;
using System.IO;
using System.Reflection;
using VelorenPort.CoreEngine;
using VelorenPort.Server;

namespace Server.Tests;

public class CliCommandTests
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
    public void ExecuteBan_AddsEntry()
    {
        var server = new GameServer(Pid.NewPid(), TimeSpan.FromMilliseconds(1), 1);
        var client = CreateClient();
        Cmd.Execute(ServerChatCommand.Ban, server, client, new[] { "cheater", "bad" });
        var field = typeof(GameServer).GetField("_settings", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var settings = (VelorenPort.Server.Settings.Settings)field.GetValue(server)!;
        Assert.NotEmpty(settings.Banlist.UuidBans());
    }

    [Fact]
    public void ExecuteStats_ReturnsInfo()
    {
        var server = new GameServer(Pid.NewPid(), TimeSpan.FromMilliseconds(1), 1);
        var client = CreateClient();
        string stats = Cmd.Execute(ServerChatCommand.Stats, server, client, Array.Empty<string>());
        Assert.Contains("players=0", stats);
    }

    [Fact]
    public void ExecuteReloadConfig_LoadsFile()
    {
        var dir = VelorenPort.Server.DataDir.DefaultDataDirName;
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "settings.json");
        File.WriteAllText(path, System.Text.Json.JsonSerializer.Serialize(new VelorenPort.Server.Settings.Settings { ServerName = "Custom" }));
        var server = new GameServer(Pid.NewPid(), TimeSpan.FromMilliseconds(1), 1);
        var client = CreateClient();
        Cmd.Execute(ServerChatCommand.ReloadConfig, server, client, Array.Empty<string>());
        var field = typeof(GameServer).GetField("_settings", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var settings = (VelorenPort.Server.Settings.Settings)field.GetValue(server)!;
        Assert.Equal("Custom", settings.ServerName);
    }
}
