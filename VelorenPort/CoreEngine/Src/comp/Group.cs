using System;

namespace VelorenPort.CoreEngine.comp;

/// <summary>
/// Primitive group identifier mirroring <c>Group</c> from Rust.
/// The values <see cref="Enemy"/> and <see cref="Npc"/> correspond to
/// default groups used by <see cref="AlignmentExtensions.DefaultGroup"/>.
/// </summary>
[Serializable]
public readonly struct Group : IEquatable<Group>
{
    public readonly uint Value;
    public Group(uint value) => Value = value;

    public bool Equals(Group other) => Value == other.Value;
    public override bool Equals(object obj) => obj is Group other && Equals(other);
    public override int GetHashCode() => (int)Value;
    public override string ToString() => $"Group({Value})";

    public static readonly Group Enemy = new(uint.MaxValue);
    public static readonly Group Npc = new(uint.MaxValue - 1);
}

/// <summary>
/// Bitflags describing privileges a group member can hold. Mirrors the flags
/// used by the Rust server for party management.
/// </summary>
[Flags]
[Serializable]
public enum GroupPrivileges
{
    None = 0,
    Invite = 1 << 0,
    Kick = 1 << 1,
    All = Invite | Kick,
}

/// <summary>
/// Event emitted when a member's privileges within a group change.
/// </summary>
[Serializable]
public struct GroupPrivilegeUpdate
{
    public Group Group;
    public Uid Member;
    public GroupPrivileges Privileges;

    public GroupPrivilegeUpdate(Group group, Uid member, GroupPrivileges privs)
    {
        Group = group;
        Member = member;
        Privileges = privs;
    }
}
