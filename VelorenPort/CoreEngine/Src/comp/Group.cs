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
