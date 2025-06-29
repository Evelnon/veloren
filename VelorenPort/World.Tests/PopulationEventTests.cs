using VelorenPort.World;
using VelorenPort.World.Site;
using VelorenPort.NativeMath;
using Xunit;

namespace World.Tests;

public class PopulationEventTests
{
    [Fact]
    public void UpdatePopulation_GeneratesBirthEvent()
    {
        var index = new WorldIndex(0);
        var site = new Site { Position = int2.zero };
        var id = index.Sites.Insert(site);
        site.Economy.Produce(new Good.Food(), 5f);
        EconomySim.UpdatePopulation(index, 1f);
        Assert.Single(index.PopulationEvents);
        Assert.Equal(PopulationEventType.Birth, index.PopulationEvents[0].Type);
    }
}
