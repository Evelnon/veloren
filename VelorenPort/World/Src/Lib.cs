using System;
using VelorenPort.World.Site;

namespace VelorenPort.World {
    /// <summary>
    /// Mirror of the root library module providing a unified generation entry.
    /// </summary>
    public static class Lib {
        public enum WorldGenerateStage {
            WorldSimGenerate,
            EconomySimulation,
            TerrainGeneration,
            SpotGeneration,
            RegionGeneration,
            All
        }

        /// <summary>Create a new world and accompanying index.</summary>
        public static (World world, WorldIndex index) Generate(uint seed) {
            return World.Generate(seed);
        }

        /// <summary>
        /// Execute one stage of world generation on an existing instance.
        /// Only a handful of stages are supported in this port.
        /// </summary>
        public static void RunStage(World world, WorldIndex index, WorldGenerateStage stage) {
            switch (stage) {
                case WorldGenerateStage.WorldSimGenerate:
                    // world already initialised
                    break;
                case WorldGenerateStage.EconomySimulation:
                    EconomySim.SimulateEconomy(index, 1f);
                    break;
                case WorldGenerateStage.TerrainGeneration:
                    foreach (var cpos in world.Sim.LoadedChunks)
                        world.Sim.Get(cpos); // ensure chunks generated
                    break;
                case WorldGenerateStage.SpotGeneration:
                    // placeholder for spot generation
                    break;
                case WorldGenerateStage.RegionGeneration:
                    // stub for region generation logic
                    break;
                case WorldGenerateStage.All:
                    RunStage(world, index, WorldGenerateStage.WorldSimGenerate);
                    RunStage(world, index, WorldGenerateStage.EconomySimulation);
                    RunStage(world, index, WorldGenerateStage.TerrainGeneration);
                    RunStage(world, index, WorldGenerateStage.SpotGeneration);
                    RunStage(world, index, WorldGenerateStage.RegionGeneration);
                    break;
            }
        }
    }
}
