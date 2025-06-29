using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine.comp;

/// <summary>
/// Basic group manager used to create simple party structures.
/// Only supports joining and leaving groups without pets or
/// advanced notifications.
/// </summary>
public class GroupManager
{
    private readonly Dictionary<Group, GroupInfo> _groups = new();
    private readonly Dictionary<Uid, Group> _membership = new();
    private uint _nextId;

    /// <summary>Retrieve information about a group if it exists.</summary>
    public GroupInfo? GetInfo(Group group)
        => _groups.TryGetValue(group, out var info) ? info : null;

    /// <summary>Return the group an entity currently belongs to, if any.</summary>
    public Group? GetGroup(Uid entity)
        => _membership.TryGetValue(entity, out var g) ? g : (Group?)null;

    /// <summary>Return true if the given entity is leader of its group.</summary>
    public bool IsLeader(Uid entity)
    {
        if (_membership.TryGetValue(entity, out var g) &&
            _groups.TryGetValue(g, out var info))
        {
            return info.Leader.Equals(entity);
        }
        return false;
    }

    /// <summary>Add <paramref name="member"/> to the group led by <paramref name="leader"/>.</summary>
    public Group JoinGroup(Uid leader, Uid member)
    {
        if (!_membership.TryGetValue(leader, out var group))
        {
            group = CreateGroup(leader);
            _membership[leader] = group;
        }

        _membership[member] = group;
        _groups[group].MemberCount++;
        return group;
    }

    /// <summary>Remove <paramref name="member"/> from its current group.</summary>
    public void LeaveGroup(Uid member)
    {
        if (!_membership.TryGetValue(member, out var group))
            return;

        if (_groups.TryGetValue(group, out var info))
        {
            info.MemberCount = Math.Max(0, info.MemberCount - 1);
            if (info.MemberCount == 0)
            {
                // remove empty group
                _groups.Remove(group);
            }
        }
        _membership.Remove(member);
    }

    private Group CreateGroup(Uid leader)
    {
        var id = new Group(_nextId++);
        _groups[id] = new GroupInfo { Leader = leader, MemberCount = 1 };
        return id;
    }
}

/// <summary>Lightweight information about a group.</summary>
public class GroupInfo
{
    public Uid Leader { get; set; }
    public int MemberCount { get; set; }
}

