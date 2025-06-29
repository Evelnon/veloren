using VelorenPort.World.Site.Gen;
using VelorenPort.World.Site;
using System.Linq;
using Unity.Mathematics;

namespace World.Tests;

public class SiteGeneratorTests
{
    [Fact]
    public void GenerateSettlement_CreatesPlazaAndHouses()
    {
        var rng = new System.Random(0);
        var site = SiteGenerator.GenerateSettlement(int2.zero, 5, rng);
        Assert.Contains(site.Plots, p => p.Kind == PlotKind.Plaza);
        Assert.True(site.Plots.Count(p => p.Kind == PlotKind.House) >= 5);
        Assert.Contains(site.Plots, p => p.Kind == PlotKind.Road);
        Assert.Contains(site.Plots, p => p.Kind == PlotKind.Farm);
    }
}
