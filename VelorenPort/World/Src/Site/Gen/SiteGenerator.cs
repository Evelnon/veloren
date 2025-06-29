using System;
using Unity.Mathematics;

namespace VelorenPort.World.Site.Gen;

/// <summary>
/// Basic settlement generator used by <see cref="VelorenPort.World.Civ.CivGenerator"/>.
/// Creates a site with a central plaza and a grid of houses connected by simple
/// road plots. This expands on the previous minimal implementation but still
/// lacks detailed decorations from the Rust version.
/// </summary>
public static class SiteGenerator
{
    /// <summary>Create a settlement at <paramref name="pos"/> with at least
    /// <paramref name="houseCount"/> houses.</summary>
    public static Site GenerateSettlement(int2 pos, int houseCount, Random rng)
    {
        var site = new Site
        {
            Position = pos,
            Origin = pos,
            Name = NameGen.Generate(rng),
            Kind = SiteKind.Settlement
        };

        // Central plaza at origin
        site.Plots.Add(new Plot { LocalPos = int2.zero, Kind = PlotKind.Plaza });

        // Place houses in a 3x3 grid around the plaza
        int placed = 0;
        for (int y = -1; y <= 1 && placed < houseCount; y++)
        for (int x = -1; x <= 1 && placed < houseCount; x++)
        {
            if (x == 0 && y == 0) continue;
            site.Plots.Add(new Plot
            {
                LocalPos = new int2(x * 4, y * 4),
                Kind = PlotKind.House
            });
            placed++;
        }


        // Simple roads along X and Y axes
        for (int d = -4; d <= 4; d += 4)
        {
            if (d == 0) continue;
            site.Plots.Add(new Plot { LocalPos = new int2(d, 0), Kind = PlotKind.Road });
            site.Plots.Add(new Plot { LocalPos = new int2(0, d), Kind = PlotKind.Road });
        }

        // Farms a short distance from the plaza
        int farmDist = 8;
        site.Plots.Add(new Plot { LocalPos = new int2(farmDist, 0), Kind = PlotKind.Farm });
        site.Plots.Add(new Plot { LocalPos = new int2(-farmDist, 0), Kind = PlotKind.Farm });
        site.Plots.Add(new Plot { LocalPos = new int2(0, farmDist), Kind = PlotKind.Farm });
        site.Plots.Add(new Plot { LocalPos = new int2(0, -farmDist), Kind = PlotKind.Farm });

        return site;
    }
}
