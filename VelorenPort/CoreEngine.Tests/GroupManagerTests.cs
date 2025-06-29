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
}

