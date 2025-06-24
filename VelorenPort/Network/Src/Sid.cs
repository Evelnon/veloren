using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Stream identifier used to differentiate channels.
    /// </summary>
    [Serializable]
    public struct Sid : IEquatable<Sid>, IComparable<Sid> {
        public ulong Value { get; private set; }

        public Sid(ulong value) {
            Value = value;
        }

        public bool Equals(Sid other) => Value == other.Value;
        public override bool Equals(object obj) => obj is Sid other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public int CompareTo(Sid other) => Value.CompareTo(other.Value);
        public override string ToString() => Value.ToString();
    }
}
