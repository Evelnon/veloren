using System;
using System.Reflection;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server;

namespace Server.Tests;

public class GroupInviteIntegrationTests
{
    [Fact]
    public void AcceptInvite_JoinGroup()
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

        server.SendInvite(inviter.Uid, invitee.Uid, InviteKind.Group);
        server.RespondToInvite(invitee.Uid, inviter.Uid, InviteKind.Group, InviteAnswer.Accepted);

        var group = server.GroupManager.GetGroup(inviter.Uid);
        Assert.NotNull(group);
        Assert.Equal(group, server.GroupManager.GetGroup(invitee.Uid));
    }
}
