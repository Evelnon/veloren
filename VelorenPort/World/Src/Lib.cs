using System;
using VelorenPort.World.Site;

namespace VelorenPort.World {
    /// <summary>
    /// Mirror of the root library module providing a unified generation entry.
    /// </summary>
    public static class Lib {
        public enum WorldGenerateStage {
            WorldSimGenerate,
            CivilizationGeneration,
            EconomySimulation,
            ErosionSimulation,
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
                case WorldGenerateStage.CivilizationGeneration:
                    Civ.CivGenerator.Generate(world, index, 3);
                    break;
                case WorldGenerateStage.EconomySimulation:
                    EconomySim.SimulateEconomy(index, 1f);
                    break;
                case WorldGenerateStage.ErosionSimulation:
                    Sim.Erosion.Apply(world.Sim);
                    break;
                case WorldGenerateStage.TerrainGeneration:
                    foreach (var cpos in world.Sim.LoadedChunks)
                        world.Sim.Get(cpos); // ensure chunks generated
                    break;
                case WorldGenerateStage.SpotGeneration:
                    // create one point of interest near each site if none exist
                    foreach (var pair in index.Sites.All)
                    {
                        var site = pair.Value;
                        if (site.PointsOfInterest.Count == 0)
                        {
                            var rnd = new Random((int)pair.Key.Value);
                            var off = new Unity.Mathematics.int2(rnd.Next(-16, 17), rnd.Next(-16, 17));
                            PoiKind kind = rnd.NextDouble() < 0.5
                                ? new PoiKind.Peak((uint)rnd.Next(50, 300))
                                : new PoiKind.Lake((uint)rnd.Next(1, 10));
                            site.PointsOfInterest.Add(new PointOfInterest
                            {
                                Position = site.Position + off,
                                Description = $"Landmark near {site.Name}",
                                Kind = kind

                            });
                        }
                    }
                    break;
                case WorldGenerateStage.RegionGeneration:
                    foreach (var cpos in world.Sim.LoadedChunks)
                        world.Sim.Regions.Get(cpos);
                    break;
                case WorldGenerateStage.All:
                    RunStage(world, index, WorldGenerateStage.WorldSimGenerate);
                    RunStage(world, index, WorldGenerateStage.CivilizationGeneration);
                    RunStage(world, index, WorldGenerateStage.EconomySimulation);
                    RunStage(world, index, WorldGenerateStage.ErosionSimulation);
                    RunStage(world, index, WorldGenerateStage.TerrainGeneration);
                    RunStage(world, index, WorldGenerateStage.SpotGeneration);
                    RunStage(world, index, WorldGenerateStage.RegionGeneration);
                    break;
            }
        }
    }
}
