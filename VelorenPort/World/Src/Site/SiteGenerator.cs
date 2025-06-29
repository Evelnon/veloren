using System;
using System.Linq;
using VelorenPort.NativeMath;
using VelorenPort.World.Site.Stats;
using VelorenPort.CoreEngine;

using VelorenPort.World.Site.Tile;
namespace VelorenPort.World.Site;

/// <summary>
/// Very small settlement generator inspired by world/src/site/gen.rs.
/// It places a plaza, a number of plots using <see cref="PlotTemplates"/>
/// and connects them with roads. Generation statistics are recorded through
/// <see cref="SitesGenMeta"/>.
/// </summary>
public static class SiteGenerator
{
    private static GenStatSiteKind ToStatKind(SiteKind kind) => kind switch
    {
        SiteKind.Terracotta => GenStatSiteKind.Terracotta,
        SiteKind.Myrmidon => GenStatSiteKind.Myrmidon,
        SiteKind.CliffTown => GenStatSiteKind.CliffTown,
        SiteKind.SavannahTown => GenStatSiteKind.SavannahTown,
        SiteKind.CoastalTown => GenStatSiteKind.CoastalTown,
        SiteKind.DesertCity => GenStatSiteKind.DesertCity,
        _ => GenStatSiteKind.City
    };

    /// <summary>
    /// Generate a small settlement at <paramref name="origin"/>.
    /// </summary>
    public static Site Generate(System.Random rng, SiteKind kind, int2 origin, SitesGenMeta? stats = null)
    {
        var site = new Site
        {
            Position = origin,
            Origin = origin,
            Name = NameGen.Generate(rng),
            Kind = kind
        };

        stats?.Add(site.Name, ToStatKind(kind));

        // Initial plaza at the origin
        AddPlot(site, PlotKind.Plaza, int2.zero, stats);

        // Some roads heading outwards from the plaza
        foreach (var dir in WorldUtil.CARDINALS)
        {
            for (int i = 1; i <= 2; i++)
                site.Tiles.Set(dir * i, new Tile { Kind = TileKind.Road });
        }

        // Random plots around the plaza
        int plotCount = rng.Next(2, 5);
        var possible = PlotTemplates.Templates.Keys
            .Where(k => k != PlotKind.Plaza && k != PlotKind.Road && k != PlotKind.Unknown)
            .ToArray();
        for (int i = 0; i < plotCount; i++)
        {
            var local = new int2(rng.Next(-4, 5), rng.Next(-4, 5));
            var pk = possible[rng.Next(possible.Length)];
            AddPlot(site, pk, local, stats);
        }

        return site;
    }

    private static GenStatPlotKind MapStatKind(PlotKind kind) => kind switch
    {
        PlotKind.Workshop or PlotKind.CoastalWorkshop or PlotKind.SavannahWorkshop => GenStatPlotKind.Workshop,
        PlotKind.GuardTower or PlotKind.CliffTower => GenStatPlotKind.GuardTower,
        PlotKind.Castle => GenStatPlotKind.Castle,
        PlotKind.AirshipDock or PlotKind.CoastalAirshipDock or PlotKind.SavannahAirshipDock or PlotKind.CliffTownAirshipDock or PlotKind.DesertCityAirshipDock => GenStatPlotKind.AirshipDock,
        PlotKind.Tavern => GenStatPlotKind.Tavern,
        PlotKind.Plaza => GenStatPlotKind.Plaza,
        _ => GenStatPlotKind.House
    };

    private static void AddPlot(Site site, PlotKind kind, int2 localPos, SitesGenMeta? stats)
    {
        var statKind = MapStatKind(kind);
        stats?.Attempt(site.Name, statKind);
        if (PlotTemplates.Templates.TryGetValue(kind, out var tmpl))
        {
            var area = new Aabr(localPos, localPos + new int2(4, 4));
            if (!site.Tiles.IsAreaFree(area))
            {
                stats?.RecordEvent(site.Name, GenStatEventKind.Warning);
                return;
            }
            site.Tiles.ApplyTemplate(localPos, tmpl);
        }
        var plot = new Plot { LocalPos = localPos, Kind = kind };
        site.Plots.Add(plot);
        DecorateAround(site, localPos, tmpl?.Keys ?? Enumerable.Empty<int2>());
        ConnectToRoad(site, localPos);
        stats?.Success(site.Name, statKind);
    }

    private static void DecorateAround(Site site, int2 origin, System.Collections.Generic.IEnumerable<int2> tiles)
    {
        foreach (var t in tiles)
        {
            foreach (var n in WorldUtil.CARDINALS.Append(new int2(1,1)).Append(new int2(-1,1)).Append(new int2(1,-1)).Append(new int2(-1,-1)))
            {
                int2 pos = origin + t + n;
                if (site.Tiles.GetKnown(pos)?.IsEmpty ?? true)
                {
                    var tile = Tile.Free(TileKind.Field);
                    if ((pos.x + pos.y) % 2 == 0)
                    {
                        tile.Sprite = SpriteKind.Tree;
                        site.Decorations.Add(new DecorationInfo(pos, SpriteKind.Tree));
                    }
                    site.Tiles.Set(pos, tile);
                }
            }
        }
    }

    private static void ConnectToRoad(Site site, int2 localPos)
    {
        int2 cur = localPos;
        while (cur.x != 0)
        {
            cur.x += cur.x > 0 ? -1 : 1;
            if (site.Tiles.GetKnown(cur)?.IsEmpty ?? true)
                site.Tiles.Set(cur, new Tile { Kind = TileKind.Road });
        }
        while (cur.y != 0)
        {
            cur.y += cur.y > 0 ? -1 : 1;
            if (site.Tiles.GetKnown(cur)?.IsEmpty ?? true)
                site.Tiles.Set(cur, new Tile { Kind = TileKind.Road });
        }
    }
}
