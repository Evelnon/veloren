using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine.ECS;
using VelorenPort.CoreEngine;

namespace VelorenPort.CoreEngine.comp;

/// <summary>
/// Components and enums related to player invites. Mirrors
/// `common/src/comp/invite.rs`.
/// </summary>
[Serializable]
public enum InviteKind
{
    Group,
    Trade,
}

[Serializable]
public enum InviteResponse
{
    Accept,
    Decline,
}

[Serializable]
public struct Invite : IComponent
{
    public Uid Inviter;
    public InviteKind Kind;

    public Invite(Uid inviter, InviteKind kind)
    {
        Inviter = inviter;
        Kind = kind;
    }
}

[Serializable]
public class PendingInvites : IComponent
{
    public List<(Uid Invitee, InviteKind Kind, DateTime Timeout)> Invites { get; } = new();

    public void Add(Uid invitee, InviteKind kind, DateTime timeout) =>
        Invites.Add((invitee, kind, timeout));
}
