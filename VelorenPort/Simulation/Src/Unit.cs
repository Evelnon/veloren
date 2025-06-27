using System;

namespace VelorenPort.Simulation {
    /// <summary>
    /// Equivalent to Rust's unit type `()`.
    /// Useful for events that do not require additional system data.
    /// </summary>
    [Serializable]
    public readonly struct Unit { }
}
