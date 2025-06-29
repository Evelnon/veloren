using Xunit;
using VelorenPort.CoreEngine.comp;
using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class GroupManagerTests
{
    [Fact]
    public void JoinGroup_CreatesGroupAndAddsMembers()
    {
        var manager = new GroupManager();
        Uid leader = new(1);
        Uid member = new(2);

        Group group = manager.JoinGroup(leader, member);

        Assert.True(manager.IsLeader(leader));
        Assert.Equal(group, manager.GetGroup(leader));
        Assert.Equal(group, manager.GetGroup(member));
        Assert.Equal(2, manager.GetInfo(group)!.MemberCount);
    }

    [Fact]
    public void LeaveGroup_RemovesAndDeletesEmptyGroup()
    {
        var manager = new GroupManager();
        Uid leader = new(1);
        Uid member = new(2);

        Group group = manager.JoinGroup(leader, member);
        manager.LeaveGroup(member);
        Assert.Null(manager.GetGroup(member));
        Assert.Equal(1, manager.GetInfo(group)!.MemberCount);

        manager.LeaveGroup(leader);
        Assert.Null(manager.GetGroup(leader));
        Assert.Null(manager.GetInfo(group));
    }

    [Fact]
    public void JoinGroup_EmitsEvents()
    {
        var manager = new GroupManager();
        Uid leader = new(1);
        Uid member = new(2);

        manager.JoinGroup(leader, member);
        var events = manager.Events.RecvAll();

        Assert.Contains(events, e => e.Action == GroupAction.Created && e.Member.Equals(leader));
        Assert.Contains(events, e => e.Action == GroupAction.Joined && e.Member.Equals(member));
    }

    [Fact]
    public void AddPet_AddsPetAndEmitsEvents()
    {
        var manager = new GroupManager();
        Uid owner = new(1);
        Uid pet = new(3);

        manager.AddPet(owner, pet);

        Assert.Equal(manager.GetGroup(owner), manager.GetGroup(pet));
        Assert.True(manager.IsPet(pet));

        var events = manager.Events.RecvAll();
        Assert.Contains(events, e => e.Action == GroupAction.Created && e.Member.Equals(owner));
        Assert.Contains(events, e => e.Action == GroupAction.PetAdded && e.Member.Equals(pet));
    }

    [Fact]
    public void RemovePet_LeavesGroupAndEmitsEvent()
    {
        var manager = new GroupManager();
        Uid owner = new(1);
        Uid pet = new(3);

        manager.AddPet(owner, pet);
        manager.Events.RecvAll(); // clear

        manager.RemovePet(pet);

        Assert.Null(manager.GetGroup(pet));
        Assert.False(manager.IsPet(pet));

        var events = manager.Events.RecvAll();
        Assert.Contains(events, e => e.Action == GroupAction.PetRemoved && e.Member.Equals(pet));
    }

    [Fact]
    public void KickMember_RemovesMemberAndEmitsEvent()
    {
        var manager = new GroupManager();
        Uid leader = new(1);
        Uid member = new(2);

        manager.JoinGroup(leader, member);
        manager.Events.RecvAll(); // clear initial

        manager.KickMember(leader, member);

        Assert.Null(manager.GetGroup(member));
        var events = manager.Events.RecvAll();
        Assert.Contains(events, e => e.Action == GroupAction.Kicked && e.Member.Equals(member));
    }
}

