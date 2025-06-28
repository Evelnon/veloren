using System;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Marker component for an entity acting as tether leader.
    /// </summary>
    [Serializable]
    public class Leader {}

    /// <summary>
    /// Marker component for an entity acting as tether follower.
    /// </summary>
    [Serializable]
    public class Follower {}

    /// <summary>
    /// Link representing a tether between two entities.
    /// Ported from <c>tether.rs</c>.
    /// </summary>
    [Serializable]
    public class Tethered {
        public Uid Leader { get; set; }
        public Uid Follower { get; set; }
        public float TetherLength { get; set; }

        public Tethered(Uid leader, Uid follower, float tetherLength) {
            Leader = leader;
            Follower = follower;
            TetherLength = tetherLength;
        }
    }

    [Serializable]
    public enum TetherError {
        None,
        NoSuchEntity,
        NotTetherable,
    }
}
