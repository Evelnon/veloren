using System;

namespace VelorenPort.World {
    /// <summary>
    /// Mirror of the root library module providing a unified generation entry.
    /// </summary>
    public static class Lib {
        public enum WorldGenerateStage {
            WorldSimGenerate,
            EconomySimulation,
            SpotGeneration,
        }

        /// <summary>Create a new world and accompanying index.</summary>
        public static (World world, WorldIndex index) Generate(uint seed) {
            return World.Generate(seed);
        }
    }
}
