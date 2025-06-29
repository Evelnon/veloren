using System;
using VelorenPort.CoreEngine;
using VelorenPort.World.Site.Stats;
using VelorenPort.NativeMath;

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
        /// <param name="stats">Optional statistics collector.</param>
        public static void Generate(World world, WorldIndex index, int count, SitesGenMeta? stats = null)
        {
            var rng = new Random((int)index.Seed);
            int2 mapSize = TerrainChunkSize.Blocks(world.Sim.GetSize());

            var kinds = Enum.GetValues<SiteKind>();

            for (int i = 0; i < count; i++)
            {
                var pos = new int2(rng.Next(0, mapSize.x), rng.Next(0, mapSize.y));
                var kind = kinds.Length > 0 ? kinds.GetValue(rng.Next(kinds.Length)) as SiteKind? ?? SiteKind.Refactor : SiteKind.Refactor;

                var site = Site.SiteGenerator.Generate(rng, kind, pos, stats);

                var siteId = index.Sites.Insert(site);
                SpawnPopulation(index, site, siteId, rng, stats);
            }
            stats?.Log();
        }

        private static void SpawnPopulation(WorldIndex index, Site.Site site, Store<Site.Site>.Id siteId, Random rng, SitesGenMeta? stats)
        {
            foreach (var _ in site.Plots)
            {
                var uid = index.AllocateUid();
                var npc = new Npc(uid)
                {
                    Name = Site.NameGen.Generate(rng),
                    Home = new SiteId(siteId.Value)
                };
                var npcId = index.Npcs.Insert(npc);
                site.Population.Add(npcId);
                stats?.RecordEvent(site.Name, GenStatEventKind.PopulationBirth);
            }
        }
    }
}
