using System;
using System.Reflection;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;
using VelorenPort.Server;
using VelorenPort.Network;
using Xunit;

namespace Server.Tests;

public class GroupUpdatePropagationTests
{
    [Fact]
    public void GroupUpdates_AreQueuedOnParticipants()
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

        var updateWorld = typeof(GameServer).GetMethod("UpdateWorld", BindingFlags.NonPublic | BindingFlags.Instance)!;
        updateWorld.Invoke(server, null);

        var ev1 = p1.TryFetchEvent();
        var ev2 = p2.TryFetchEvent();
        Assert.NotNull(ev1);
        Assert.NotNull(ev2);
        Assert.IsType<ParticipantEvent.GroupUpdate>(ev1!);
        Assert.IsType<ParticipantEvent.GroupUpdate>(ev2!);
    }
}

