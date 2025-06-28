using System;

namespace VelorenPort.Server.Rtsim {
    /// <summary>
    /// Small placeholder for the real-time simulation used by Veloren. The
    /// original Rust module performs heavy world calculations. Here we only
    /// track when the simulation was started.
    /// </summary>
    public class RtSim {
        public DateTime Started { get; } = DateTime.UtcNow;
    }
}
