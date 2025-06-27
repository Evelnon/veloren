using System;
using VelorenPort.CoreEngine;

namespace VelorenPort.World.Site {
    /// <summary>
    /// Entry point for a very small scale economy simulation.
    /// Each registered site advances its internal state every tick.
    /// </summary>
    public static class Economy {
        /// <summary>
        /// Progress the economy of all sites contained in <paramref name="index"/>.
        /// </summary>
        public static void SimulateEconomy(WorldIndex index, float dt) {
            foreach (var (id, site) in index.Sites.Enumerate()) {
                site.Economy.Tick(dt);
            }
        }
    }
}
