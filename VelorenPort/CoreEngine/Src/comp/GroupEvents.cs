using System;

namespace VelorenPort.CoreEngine.comp;

/// <summary>Actions that can occur within a group.</summary>
public enum GroupAction
{
    Created,
    Joined,
    Left,
    Disbanded,
    PetAdded,
    PetRemoved,
    Kicked
}

/// <summary>Notification emitted for group changes.</summary>
[Serializable]
public struct GroupEvent
{
    public Group Group;
    public VelorenPort.CoreEngine.Uid Member;
    public GroupAction Action;

    public GroupEvent(Group group, VelorenPort.CoreEngine.Uid member, GroupAction action)
    {
        Group = group;
        Member = member;
        Action = action;
    }
}
