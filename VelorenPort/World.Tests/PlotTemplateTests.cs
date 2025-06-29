using VelorenPort.World.Site;
using VelorenPort.NativeMath;

namespace World.Tests;

public class PlotTemplateTests
{
    [Fact]
    public void Templates_ContainExpectedStructures()
    {
        Assert.True(PlotTemplates.Templates.ContainsKey(PlotKind.Tavern));
        Assert.True(PlotTemplates.Templates.ContainsKey(PlotKind.GuardTower));
        Assert.True(PlotTemplates.Templates.ContainsKey(PlotKind.Castle));

        var tavern = PlotTemplates.Templates[PlotKind.Tavern];
        Assert.True(tavern.Count >= 9);
        Assert.Contains(new int2(1,1), tavern.Keys);

        var tower = PlotTemplates.Templates[PlotKind.GuardTower];
        Assert.Contains(new int2(0,0), tower.Keys);
    }
}
