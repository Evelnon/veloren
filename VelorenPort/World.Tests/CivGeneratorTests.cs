using VelorenPort.World;
using VelorenPort.World.Civ;

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
}
