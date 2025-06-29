using System;
using Unity.Mathematics;

namespace VelorenPort.World.Civ
{
    /// <summary>
    /// Very small scale civilisation generator. Places a number of
    /// <see cref="Site.Site"/> objects across the world using a deterministic
    /// random generator based on the world seed.
    /// </summary>
    public static class CivGenerator
    {
        /// <summary>Create <paramref name="count"/> sites with random names.</summary>
        public static void Generate(World world, WorldIndex index, int count)
        {
            var rng = new Random((int)index.Seed);
            int2 mapSize = TerrainChunkSize.Blocks(world.Sim.GetSize());

            for (int i = 0; i < count; i++)
            {
                var pos = new int2(rng.Next(0, mapSize.x), rng.Next(0, mapSize.y));
                string name = Site.NameGen.Generate(rng);
                var site = new Site.Site
                {
                    Position = pos,
                    Origin = pos,
                    Name = name,
                    Kind = Site.SiteKind.Refactor
                };

                int plotCount = rng.Next(1, 4);
                for (int p = 0; p < plotCount; p++)
                {
                    var plot = new Site.Plot
                    {
                        LocalPos = new int2(rng.Next(-2, 3), rng.Next(-2, 3)),
                        Kind = Site.PlotKind.House
                    };
                    site.Plots.Add(plot);
                }

                index.Sites.Insert(site);
            }
        }
    }
}
