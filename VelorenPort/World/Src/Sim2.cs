using System;
using VelorenPort.World.Site;

namespace VelorenPort.World {
    /// <summary>
    /// Alternative simulation entry mirroring <c>sim2.rs</c>.
    /// </summary>
    public static class Sim2 {
        /// <summary>
        /// Run the simplified economy simulation on the given world index.
        /// Mirrors <c>sim2.rs</c> which simply forwards to the economy module.
        /// </summary>
        public static void Simulate(WorldIndex index, WorldSim world) {
            Economy.SimulateEconomy(index);
        }
    }
}
