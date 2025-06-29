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
            Assert.NotEmpty(site.Plots);
            Assert.Contains(site.Plots, p => p.Kind == Site.PlotKind.Road);
        }
    }
}
