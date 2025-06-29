using VelorenPort.World;
using VelorenPort.World.Civ;
using VelorenPort.NativeMath;
using System.Linq;
using Xunit;

namespace World.Tests;

public class CivGeneratorTests
{
    [Fact]
    public void Generate_AddsExpectedNumberOfSites()
    {
        var (world, index) = World.Empty();
        CivGenerator.Generate(world, index, 4);
        Assert.Equal(4, index.Sites.Items.Count);
        foreach (var (_, site) in index.Sites.Enumerate())
        {
            Assert.False(string.IsNullOrWhiteSpace(site.Name));
        }
    }

    [Fact]
    public void Generate_BasicLayoutCreated()
    {
        var (world, index) = World.Empty();
        CivGenerator.Generate(world, index, 1);
        var site = index.Sites.Items[0];
        Assert.Equal(Site.TileKind.Plaza, site.Tiles.Get(int2.zero).Kind);
        Assert.Equal(Site.TileKind.Road, site.Tiles.Get(new int2(1, 0)).Kind);

        foreach (var plot in site.Plots)
        {
            int2 p = plot.LocalPos;
            bool connected = false;
            int2 cur = p;
            while (cur.x != 0)
            {
                cur.x += cur.x > 0 ? -1 : 1;
                if (site.Tiles.Get(cur).Kind == Site.TileKind.Road)
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
                    if (site.Tiles.Get(cur).Kind == Site.TileKind.Road)
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
    public void Generate_CreatesNpcPopulation()
    {
        var (world, index) = World.Empty();
        CivGenerator.Generate(world, index, 2);
        Assert.True(index.Npcs.Items.Count > 0);
        foreach (var (_, site) in index.Sites.Enumerate())
            Assert.True(site.Population.Count > 0);
    }

    [Fact]
    public void Generate_RecordsPopulationBirthEvent()
    {
        var (world, index) = World.Empty();
        var stats = new SitesGenMeta(42);
        CivGenerator.Generate(world, index, 1, stats);
        var site = index.Sites.Items[0];
        Assert.True(stats.GetEventCount(site.Name, GenStatEventKind.PopulationBirth) > 0);
    }

    [Fact]
    public void Generate_CreatesTradingRoute()
    {
        var (world, index) = World.Empty();
        CivGenerator.Generate(world, index, 3);
        Assert.NotEmpty(index.TradingRoutes);
        Assert.True(index.TradingRoutes[0].Sites.Count >= 3);
    }

    [Fact]
    public void Generate_RecordsBirthEventsOnIndex()
    {
        var (world, index) = World.Empty();
        CivGenerator.Generate(world, index, 1);
        Assert.NotEmpty(index.PopulationEvents);
    }

    [Fact]
    public void Generate_RecordsPlotEvents()
    {
        var (world, index) = World.Empty();
        CivGenerator.Generate(world, index, 2);
        Assert.NotEmpty(index.PlotEvents);
        Assert.Equal(index.PlotEvents.Count, index.PlotEvents.Select(e => e).Count());
    }

    [Fact]
    public void Generate_VarietyOfPlotKinds()
    {
        var (world, index) = World.Empty();
        CivGenerator.Generate(world, index, 1);
        var kinds = index.PlotEvents.Select(e => e.Kind).Distinct().ToList();
        Assert.True(kinds.Count > 1);
    }

    [Fact]
    public void Generate_PlacesDecorations()
    {
        var (world, index) = World.Empty();
        CivGenerator.Generate(world, index, 1);
        Assert.NotEmpty(index.DecorationEvents);
    }
}
