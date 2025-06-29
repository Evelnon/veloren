using System;
using VelorenPort.CoreEngine.ECS;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Partial port of the event module providing just a few event
    /// variants needed by other systems. Most complex variants from
    /// the original Rust code are omitted.
    /// </summary>
    [Serializable]
    public enum LocalEvent {
        Jump,
        ApplyImpulse,
        Boost,
        CreateOutcome,
    }

    [Serializable]
    public struct ApplyImpulseData {
        public Entity Entity;
        public float3 Impulse;
    }

    [Serializable]
    public struct BoostData {
        public Entity Entity;
        public float3 Velocity;
    }
}
