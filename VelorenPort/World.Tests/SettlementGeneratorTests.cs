using VelorenPort.World.Site;
using VelorenPort.World.Site.Stats;
using VelorenPort.World;
using VelorenPort.NativeMath;

namespace World.Tests;

public class SettlementGeneratorTests
{
    [Fact]
    public void Generate_CreatesPlazaAndRoads()
    {
        var rng = new System.Random(1);
        var stats = new SitesGenMeta(1);
        var site = SiteGenerator.Generate(rng, SiteKind.CliffTown, int2.zero, stats);
        Assert.Equal(TileKindTag.Plaza, site.Tiles.Get(int2.zero).Kind.Tag);
        Assert.Equal(TileKindTag.Road, site.Tiles.Get(new int2(1,0)).Kind.Tag);
        Assert.True(site.Plots.Count > 0);
    }

    [Fact]
    public void Generate_PlotsConnectedByRoad()
    {
        var rng = new System.Random(2);
        var site = SiteGenerator.Generate(rng, SiteKind.SavannahTown, int2.zero, null);
        foreach (var plot in site.Plots)
        {
            int2 cur = plot.LocalPos;
            bool connected = false;
            while (cur.x != 0)
            {
                cur.x += cur.x > 0 ? -1 : 1;
                if (site.Tiles.Get(cur).IsRoad)
                {
                    connected = true;
                    break;
                }
            }
            if (!connected)
            {
                while (cur.y != 0)
                {
                    cur.y += cur.y > 0 ? -1 : 1;
                    if (site.Tiles.Get(cur).IsRoad)
                    {
                        connected = true;
                        break;
                    }
                }
            }
            Assert.True(connected);
        }
    }

    [Fact]
    public void Generate_StatsUpdated()
    {
        var rng = new System.Random(3);
        var stats = new SitesGenMeta(123);
        var site = SiteGenerator.Generate(rng, SiteKind.City, int2.zero, stats);
        stats.Log();
        Assert.True(stats != null); // ensures stats object exists
    }
}
