using System;
using System.Reflection;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.Server;
using VelorenPort.Network;

namespace Server.Tests;

public class CmdTests
{
    [Fact]
    public void TryParse_ParsesCommandAndArgs()
    {
        Assert.True(Cmd.TryParse("/say hello world", out var cmd, out var args));
        Assert.Equal(ServerChatCommand.Say, cmd);
        Assert.Equal(new[] { "hello", "world" }, args);
    }

    [Fact]
    public void ExecuteTeleport_UpdatesClientPosition()
    {
        var server = new GameServer(Pid.NewPid(), TimeSpan.FromMilliseconds(1), 1);
        var participant = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(1), Guid.NewGuid(), null, null, null })!;
        var client = (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { participant })!;

        Cmd.Execute(ServerChatCommand.Teleport, server, client, new[] { "1", "2", "3" });
        Assert.Equal(new float3(1, 2, 3), client.Position.Value);
    }

    [Fact]
    public void ExecuteSetWaypoint_StoresClientWaypoint()
    {
        var server = new GameServer(Pid.NewPid(), TimeSpan.FromMilliseconds(1), 1);
        var participant = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(1), Guid.NewGuid(), null, null, null })!;
        var client = (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { participant })!;
        client.SetPosition(new float3(4, 5, 6));

        Cmd.Execute(ServerChatCommand.SetWaypoint, server, client, Array.Empty<string>());

        Assert.NotNull(client.Waypoint);
        Assert.Equal(new float3(4, 5, 6), client.Waypoint!.Value.Position);
    }

    [Fact]
    public void ExecuteOnline_ListsConnectedClients()
    {
        var server = new GameServer(Pid.NewPid(), TimeSpan.FromMilliseconds(1), 1);
        var p1 = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(1), Guid.NewGuid(), null, null, null })!;
        var c1 = (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { p1 })!;
        var p2 = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(2), Guid.NewGuid(), null, null, null })!;
        var c2 = (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { p2 })!;
        var field = typeof(GameServer).GetField("_clients", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var list = (System.Collections.IList)field.GetValue(server)!;
        list.Add(c1);
        list.Add(c2);

        string result = Cmd.Execute(ServerChatCommand.Online, server, c1, Array.Empty<string>());
        Assert.Contains(p1.Id.Value.ToString(), result);
        Assert.Contains(p2.Id.Value.ToString(), result);
    }

    [Fact]
    public void ExecuteInvite_AddsPendingInvite()
    {
        var server = new GameServer(Pid.NewPid(), TimeSpan.FromMilliseconds(1), 1);
        var p1 = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(1), Guid.NewGuid(), null, null, null })!;
        var inviter = (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { p1 })!;
        var p2 = (Participant)Activator.CreateInstance(
            typeof(Participant), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { Pid.NewPid(), new ConnectAddr.Mpsc(2), Guid.NewGuid(), null, null, null })!;
        var invitee = (Client)Activator.CreateInstance(
            typeof(Client), BindingFlags.NonPublic | BindingFlags.Instance,
            new object?[] { p2 })!;
        var list = (System.Collections.IList)typeof(GameServer).GetField("_clients", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(server)!;
        list.Add(inviter);
        list.Add(invitee);

        Cmd.Execute(ServerChatCommand.Invite, server, inviter, new[] { invitee.Uid.Value.ToString(), "Group" });

        Assert.Single(inviter.PendingInvites.Invites);
    }
}
