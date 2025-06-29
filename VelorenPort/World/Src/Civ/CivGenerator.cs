using System;
using VelorenPort.CoreEngine;
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

            var kinds = Enum.GetValues<SiteKind>();

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

                AddBasicLayout(site);

                var siteId = index.Sites.Insert(site);
                SpawnPopulation(index, site, siteId, rng);
            }
        }

        private static void AddBasicLayout(Site.Site site)
        {
            site.Tiles.Set(int2.zero, new Site.Tile { Kind = Site.TileKind.Plaza });
            for (int i = -2; i <= 2; i++)
            {
                site.Tiles.Set(new int2(i, 0), new Site.Tile { Kind = Site.TileKind.Road });
                site.Tiles.Set(new int2(0, i), new Site.Tile { Kind = Site.TileKind.Road });
            }

            foreach (var plot in site.Plots)
            {
                int2 p = plot.LocalPos;
                for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var tpos = p + new int2(dx, dy);
                    if (site.Tiles.GetKnown(tpos)?.IsEmpty ?? true)
                        site.Tiles.Set(tpos, new Site.Tile { Kind = Site.TileKind.Field });
                }

                int2 cur = p;
                while (cur.x != 0)
                {
                    cur.x += cur.x > 0 ? -1 : 1;
                    if (site.Tiles.GetKnown(cur)?.IsEmpty ?? true)
                        site.Tiles.Set(cur, new Site.Tile { Kind = Site.TileKind.Road });
                }
                while (cur.y != 0)
                {
                    cur.y += cur.y > 0 ? -1 : 1;
                    if (site.Tiles.GetKnown(cur)?.IsEmpty ?? true)
                        site.Tiles.Set(cur, new Site.Tile { Kind = Site.TileKind.Road });
                }
            }
        }

        private static void SpawnPopulation(WorldIndex index, Site.Site site, Store<Site.Site>.Id siteId, Random rng)
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
            }
        }
    }
}
