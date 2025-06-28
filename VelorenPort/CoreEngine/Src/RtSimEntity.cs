using System;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public struct RtSimEntity : IEquatable<RtSimEntity>
    {
        public NpcId Value;

        public RtSimEntity(NpcId value) { Value = value; }

        public bool Equals(RtSimEntity other) => Value.Equals(other.Value);
        public override bool Equals(object? obj) => obj is RtSimEntity other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.Value.ToString();

        public static implicit operator NpcId(RtSimEntity id) => id.Value;
        public static implicit operator RtSimEntity(NpcId value) => new(value);
        public static explicit operator int(RtSimEntity id) => id.Value.Value;
        public static explicit operator RtSimEntity(int value) => new(new NpcId(value));
    }

}
