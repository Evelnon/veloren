using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Strongly typed wrapper around an integer identifier.
    /// </summary>
    [Serializable]
    public readonly struct Typed<T> : IEquatable<Typed<T>>, IComparable<Typed<T>> {
        private readonly int _value;
        public Typed(int value) => _value = value;
        public int ToInt() => _value;
        public bool Equals(Typed<T> other) => _value == other._value;
        public override bool Equals(object? obj) => obj is Typed<T> other && Equals(other);
        public override int GetHashCode() => _value;
        public static bool operator ==(Typed<T> a, Typed<T> b) => a._value == b._value;
        public static bool operator !=(Typed<T> a, Typed<T> b) => a._value != b._value;
        public static Typed<T> operator +(Typed<T> a, int b) => new Typed<T>(a._value + b);
        public static Typed<T> operator -(Typed<T> a, int b) => new Typed<T>(a._value - b);
        public override string ToString() => _value.ToString();

        public static implicit operator int(Typed<T> t) => t._value;
        public static explicit operator Typed<T>(int v) => new Typed<T>(v);

        public static Typed<T> FromInt(int v) => new Typed<T>(v);

        public int CompareTo(Typed<T> other) => _value.CompareTo(other._value);

        public static bool TryParse(string s, out Typed<T> value) {
            if (int.TryParse(s, out var v)) {
                value = new Typed<T>(v);
                return true;
            }
            value = default;
            return false;
        }

        public static Typed<T> Parse(string s) => new Typed<T>(int.Parse(s));
    }
}
