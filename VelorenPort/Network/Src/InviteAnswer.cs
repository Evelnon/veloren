using System;

namespace VelorenPort.Network;

/// <summary>
/// Response to an invite. Mirrors `InviteAnswer` in the Rust project.
/// </summary>
[Serializable]
public enum InviteAnswer
{
    Accepted,
    Declined,
    TimedOut,
}
