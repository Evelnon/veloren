using VelorenPort.World.Site;

namespace World.Tests;

public class NameGenTests
{
    [Fact]
    public void Generate_ProducesNonEmptyCapitalizedName()
    {
        var rng = new System.Random(42);
        string name = NameGen.Generate(rng);
        Assert.False(string.IsNullOrWhiteSpace(name));
        Assert.True(char.IsUpper(name[0]));
    }

    [Fact]
    public void Generate_DifferentSeedsGiveDifferentNames()
    {
        var a = NameGen.Generate(new System.Random(1));
        var b = NameGen.Generate(new System.Random(2));
        Assert.NotEqual(a, b);
    }
}
