using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine.comp;

/// <summary>
/// Basic group manager used to create simple party structures.
/// Now exposes change notifications via an <see cref="EventBus{T}"/>
/// and supports simple pet membership.
/// </summary>
public class GroupManager
{
    private readonly Dictionary<Group, GroupInfo> _groups = new();
    private readonly Dictionary<Uid, Group> _membership = new();
    private readonly Dictionary<Uid, Uid> _petOwners = new();
    private readonly EventBus<GroupEvent> _events = new();
    private readonly EventBus<GroupPrivilegeUpdate> _privEvents = new();
    private uint _nextId;

    /// <summary>Bus emitting group change notifications.</summary>
    public EventBus<GroupEvent> Events => _events;

    /// <summary>Bus emitting privilege change notifications.</summary>
    public EventBus<GroupPrivilegeUpdate> PrivilegeEvents => _privEvents;

    /// <summary>Return true if <paramref name="entity"/> is a registered pet.</summary>
    public bool IsPet(Uid entity) => _petOwners.ContainsKey(entity);

    /// <summary>Try get the owner of a pet.</summary>
    public bool TryGetOwner(Uid pet, out Uid owner) => _petOwners.TryGetValue(pet, out owner);

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
        var created = false;
        if (!_membership.TryGetValue(leader, out var group))
        {
            group = CreateGroup(leader);
            _membership[leader] = group;
            created = true;
        }

        if (_membership.TryGetValue(member, out var existing) && existing.Equals(group))
            return group;

        _membership[member] = group;
        var info = _groups[group];
        info.MemberCount++;
        info.Privileges[member] = GroupPrivileges.None;

        if (created)
            _events.EmitNow(new GroupEvent(group, leader, GroupAction.Created));

        if (!member.Equals(leader))
            _events.EmitNow(new GroupEvent(group, member, GroupAction.Joined));

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
                _groups.Remove(group);
                _events.EmitNow(new GroupEvent(group, member, GroupAction.Disbanded));
            }
        }

        _membership.Remove(member);
        _petOwners.Remove(member);
        if (_groups.TryGetValue(group, out var info2))
            info2.Privileges.Remove(member);
        _events.EmitNow(new GroupEvent(group, member, GroupAction.Left));
    }

    private Group CreateGroup(Uid leader)
    {
        var id = new Group(_nextId++);
        _groups[id] = new GroupInfo
        {
            Leader = leader,
            MemberCount = 1,
            Privileges = new Dictionary<Uid, GroupPrivileges>
            {
                [leader] = GroupPrivileges.All
            }
        };
        return id;
    }

    /// <summary>Add a pet to the owner's group, creating it if necessary.</summary>
    public Group AddPet(Uid owner, Uid pet)
    {
        var group = JoinGroup(owner, pet);
        _petOwners[pet] = owner;
        _events.EmitNow(new GroupEvent(group, pet, GroupAction.PetAdded));
        return group;
    }

    /// <summary>Remove a pet from its group.</summary>
    public void RemovePet(Uid pet)
    {
        bool hadOwner = _petOwners.Remove(pet);
        LeaveGroup(pet);
        if (hadOwner)
            _events.EmitNow(new GroupEvent(default, pet, GroupAction.PetRemoved));
    }

    /// <summary>Kick <paramref name="member"/> from the leader's group.</summary>
    public void KickMember(Uid leader, Uid member)
    {
        if (!IsLeader(leader))
            return;
        if (!_membership.TryGetValue(member, out var group))
            return;
        if (!_membership.TryGetValue(leader, out var leaderGroup) || !leaderGroup.Equals(group))
            return;
        if (member.Equals(leader))
            return;

        LeaveGroup(member);
        _events.EmitNow(new GroupEvent(group, member, GroupAction.Kicked));
    }

    /// <summary>Change the privileges of a group member.</summary>
    public void SetPrivileges(Uid leader, Uid member, GroupPrivileges privileges)
    {
        if (!IsLeader(leader))
            return;
        if (!_membership.TryGetValue(member, out var group))
            return;
        if (!_membership.TryGetValue(leader, out var leaderGroup) || !leaderGroup.Equals(group))
            return;

        var info = _groups[group];
        info.Privileges[member] = privileges;
        _privEvents.EmitNow(new GroupPrivilegeUpdate(group, member, privileges));
    }
}

/// <summary>Lightweight information about a group.</summary>
public class GroupInfo
{
    public Uid Leader { get; set; }
    public int MemberCount { get; set; }
    public Dictionary<Uid, GroupPrivileges> Privileges { get; set; } = new();
}

