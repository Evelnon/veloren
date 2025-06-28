using System;

namespace VelorenPort.Network {
    /// <summary>
    /// Unique identifier for a network participant. Mirrors Pid from the Rust crate.
    /// </summary>
    [Serializable]
    public struct Pid : IEquatable<Pid> {
        private readonly Guid _value;

        public Pid(Guid value) {
            _value = value;
        }

        public static Pid NewPid() => new Pid(Guid.NewGuid());

        public byte[] ToByteArray() => _value.ToByteArray();

        public override string ToString() => _value.ToString("N");

        public bool Equals(Pid other) => _value.Equals(other._value);
        public override bool Equals(object obj) => obj is Pid other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
    }
}
