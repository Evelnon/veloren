using System;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public struct CharacterId : IEquatable<CharacterId>
    {
        public long Value;
        public CharacterId(long value) { Value = value; }
        public bool Equals(CharacterId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is CharacterId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
        public static implicit operator long(CharacterId id) => id.Value;
        public static implicit operator CharacterId(long value) => new CharacterId(value);
    }
}
