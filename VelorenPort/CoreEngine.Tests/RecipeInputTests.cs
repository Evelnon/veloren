using VelorenPort.CoreEngine;
using System.Collections.Generic;

namespace CoreEngine.Tests;

public class RecipeInputTests
{
    [Fact]
    public void Construct_ListSameItem()
    {
        var input = new RecipeInput.ListSameItem(new List<string> { "a", "b" });
        Assert.Equal(2, input.Items.Count);
    }
}
