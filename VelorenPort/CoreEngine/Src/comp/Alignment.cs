using System;

namespace VelorenPort.CoreEngine.comp;

/// <summary>
/// Alignment of an entity used for hostility checks.
/// Mirrors the Rust enum with an Owned variant carrying a Uid.
/// </summary>
[Serializable]
public abstract record Alignment
{
    public sealed record Wild : Alignment;
    public sealed record Enemy : Alignment;
    public sealed record Npc : Alignment;
    public sealed record Tame : Alignment;
    public sealed record Owned(Uid Owner) : Alignment;
    public sealed record Passive : Alignment;
}

public static class AlignmentExtensions
{
    public static bool HostileTowards(this Alignment self, Alignment other) => (self, other) switch
    {
        (Alignment.Passive, _) => false,
        (_, Alignment.Passive) => false,
        (Alignment.Enemy, Alignment.Enemy) => false,
        (Alignment.Enemy, Alignment.Wild) => false,
        (Alignment.Wild, Alignment.Enemy) => false,
        (Alignment.Wild, Alignment.Wild) => false,
        (Alignment.Npc, Alignment.Wild) => false,
        (Alignment.Npc, Alignment.Enemy) => true,
        (_, Alignment.Enemy) => true,
        (Alignment.Enemy, _) => true,
        _ => false,
    };

    public static bool PassiveTowards(this Alignment self, Alignment other) => (self, other) switch
    {
        (Alignment.Enemy, Alignment.Enemy) => true,
        (Alignment.Owned a, Alignment.Owned b) when a.Owner.Equals(b.Owner) => true,
        (Alignment.Npc, Alignment.Npc) => true,
        (Alignment.Npc, Alignment.Tame) => true,
        (Alignment.Enemy, Alignment.Wild) => true,
        (Alignment.Wild, Alignment.Enemy) => true,
        (Alignment.Tame, Alignment.Npc) => true,
        (Alignment.Tame, Alignment.Tame) => true,
        (_, Alignment.Passive) => true,
        _ => false,
    };

    public static bool FriendlyTowards(this Alignment self, Alignment other) => (self, other) switch
    {
        (Alignment.Enemy, Alignment.Enemy) => true,
        (Alignment.Owned a, Alignment.Owned b) when a.Owner.Equals(b.Owner) => true,
        (Alignment.Npc, Alignment.Npc) => true,
        (Alignment.Npc, Alignment.Tame) => true,
        (Alignment.Tame, Alignment.Npc) => true,
        (Alignment.Tame, Alignment.Tame) => true,
        (_, Alignment.Passive) => true,
        _ => false,
    };

    public static Group? DefaultGroup(this Alignment self) => self switch
    {
        Alignment.Enemy => Group.Enemy,
        Alignment.Npc => Group.Npc,
        Alignment.Tame => Group.Npc,
        _ => null,
    };
}
