using System;
using System.Collections.Generic;

namespace VelorenPort.Simulation {
    /// <summary>
    /// Minimal representation of a site used for NPC population tracking.
    /// </summary>
    [Serializable]
    public class Site {
        public HashSet<NpcId> Population { get; } = new();
    }
}
