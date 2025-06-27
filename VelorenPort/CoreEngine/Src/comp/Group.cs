using System;

namespace VelorenPort.CoreEngine.comp {
    /// <summary>
    /// Simple group identifier. Mirrors `Group` from `common/src/comp/group.rs`.
    /// </summary>
    [Serializable]
    public readonly struct Group : IEquatable<Group> {
        public readonly uint Value;
        public Group(uint value) { Value = value; }

        public bool Equals(Group other) => Value == other.Value;
        public override bool Equals(object? obj) => obj is Group g && Equals(g);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static readonly Group ENEMY = new(uint.MaxValue);
        public static readonly Group NPC = new(uint.MaxValue - 1);

        public static implicit operator uint(Group g) => g.Value;
        public static explicit operator Group(uint v) => new(v);
    }
}
