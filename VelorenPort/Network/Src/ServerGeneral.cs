using System;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;

namespace VelorenPort.Network;

/// <summary>
/// General server messages sent to clients. Only a minimal subset is
/// implemented so far.
/// </summary>
[Serializable]
public abstract record ServerGeneral
{
    [Serializable]
    public sealed record Invite(Uid Inviter, TimeSpan Timeout, InviteKind Kind) : ServerGeneral;

    [Serializable]
    public sealed record InvitePending(Uid Invitee) : ServerGeneral;

    [Serializable]
    public sealed record InviteComplete(Uid Target, InviteAnswer Answer, InviteKind Kind) : ServerGeneral;

    [Serializable]
    public sealed record ChatMsg(comp.ChatMsg Msg) : ServerGeneral;

    [Serializable]
    public sealed record WeatherUpdate(Weather Weather) : ServerGeneral;
}
