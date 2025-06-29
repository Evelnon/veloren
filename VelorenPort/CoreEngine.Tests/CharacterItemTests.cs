using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.comp;
using Xunit;

namespace CoreEngine.Tests;

public class CharacterItemTests
{
    [Fact]
    public void ConstructorAssignsFields()
    {
        var chara = new Character(new CharacterId(1), "Hero");
        var inv = new ReducedInventory();
        var item = new CharacterItem(chara, Body.Humanoid, true, inv, "Valenoar");
        Assert.Equal(chara, item.Character);
        Assert.Equal(Body.Humanoid, item.Body);
        Assert.True(item.Hardcore);
        Assert.Equal(inv, item.Inventory);
        Assert.Equal("Valenoar", item.Location);
    }
}
