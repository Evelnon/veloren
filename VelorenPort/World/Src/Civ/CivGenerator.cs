using System;
using System.Collections.Generic;
using System.Linq;
using VelorenPort.CoreEngine;
using VelorenPort.World.Site.Stats;
using VelorenPort.NativeMath;
using VelorenPort.World.Site;
using System.Collections.Generic;

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
            var worldDims = world.Sim.GetAabr();
            var reqs = new ProximityRequirementsBuilder().Finalize(worldDims);

            var routeSites = new List<Store<Site.Site>.Id>();

            var kinds = Enum.GetValues<SiteKind>();

            var worldBounds = new Aabr(int2.zero, mapSize);

            for (int i = 0; i < count; i++)
            {
                var existing = index.Sites.Items.Select(s => s.Position);
                var reqs = new ProximityRequirementsBuilder()
                    .AvoidAllOf(existing, 20)
                    .Finalize(worldBounds);
                var pos = FindSiteLocation(rng, reqs);
                var kind = kinds.Length > 0 ? kinds.GetValue(rng.Next(kinds.Length)) as SiteKind? ?? SiteKind.Refactor : SiteKind.Refactor;

                var site = Site.SiteGenerator.Generate(rng, kind, pos, stats);

                var siteId = index.Sites.Insert(site);
                routeSites.Add(siteId);
                RecordPlots(index, site, siteId, stats);
                RecordDecorations(index, site, siteId, stats);
                SpawnPopulation(index, site, siteId, rng, stats);
            }
            if (routeSites.Count > 1)
                EconomySim.AddTradingRoute(index, new TradingRoute(routeSites));

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
                index.RecordPopulationEvent(new PopulationEvent(PopulationEventType.Birth, npcId, siteId));
            }
        }

        private static void RecordPlots(WorldIndex index, Site.Site site, Store<Site.Site>.Id siteId, SitesGenMeta? stats = null)
        {
            foreach (var plot in site.Plots)
            {
                index.RecordPlotEvent(new PlotCreatedEvent(siteId, plot.Kind, plot.LocalPos));
                stats?.RecordEvent(site.Name, GenStatEventKind.PlotCreated);
            }
        }

        private static void RecordDecorations(WorldIndex index, Site.Site site, Store<Site.Site>.Id siteId, SitesGenMeta? stats = null)
        {
            foreach (var deco in site.Decorations)
            {
                var ev = new DecorationPlacedEvent(siteId, deco.LocalPos, deco.Sprite);
                index.RecordDecorationEvent(ev);
                stats?.RecordEvent(site.Name, GenStatEventKind.DecorationPlaced);
            }
        }

        private static int2 FindSiteLocation(Random rng, ProximityRequirements reqs)
        {
            const int MaxAttempts = 1000;
            for (int i = 0; i < MaxAttempts; i++)
            {
                var hint = reqs.LocationHint;
                var pos = new int2(
                    rng.Next(hint.Min.x, hint.Max.x),
                    rng.Next(hint.Min.y, hint.Max.y));
                if (reqs.SatisfiedBy(pos))
                    return pos;
            }

            // fallback if nothing matched
            return new int2(rng.Next(reqs.LocationHint.Min.x, reqs.LocationHint.Max.x),
                            rng.Next(reqs.LocationHint.Min.y, reqs.LocationHint.Max.y));

        }
    }
}
