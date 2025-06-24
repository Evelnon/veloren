using System;

namespace VelorenPort.CoreEngine {
    [Serializable]
    public struct RtSimEntity : IEquatable<RtSimEntity>
    {
        public int Value; // Placeholder for NpcId
        public RtSimEntity(int value) { Value = value; }
        public bool Equals(RtSimEntity other) => Value == other.Value;
        public override bool Equals(object obj) => obj is RtSimEntity other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
        public static implicit operator int(RtSimEntity id) => id.Value;
        public static implicit operator RtSimEntity(int value) => new RtSimEntity(value);
    }

}
