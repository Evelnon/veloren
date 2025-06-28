using System;

namespace VelorenPort.CoreEngine
{
    /// <summary>
    /// Input kind for abilities and general actions. Mirrors the Rust enum but
    /// uses a discriminated structure to allow ability slots.
    /// </summary>
    [Serializable]
    public readonly struct InputKind : IEquatable<InputKind>
    {
        public enum Kind
        {
            Primary,
            Secondary,
            Block,
            Ability,
            Roll,
            Jump,
            Fly,
        }

        public Kind Type { get; }
        public int Index { get; }

        public InputKind(Kind type, int index = 0)
        {
            Type = type;
            Index = index;
        }

        public static InputKind Primary => new(Kind.Primary);
        public static InputKind Secondary => new(Kind.Secondary);
        public static InputKind Block => new(Kind.Block);
        public static InputKind Roll => new(Kind.Roll);
        public static InputKind Jump => new(Kind.Jump);
        public static InputKind Fly => new(Kind.Fly);
        public static InputKind AbilitySlot(int index) => new(Kind.Ability, index);

        public bool IsAbility => Type is Kind.Primary or Kind.Secondary or Kind.Block or Kind.Ability;

        public bool Equals(InputKind other) => Type == other.Type && Index == other.Index;
        public override bool Equals(object obj) => obj is InputKind other && Equals(other);
        public override int GetHashCode() => HashCode.Combine((int)Type, Index);
        public override string ToString() => Type == Kind.Ability ? $"Ability({Index})" : Type.ToString();
    }
}
