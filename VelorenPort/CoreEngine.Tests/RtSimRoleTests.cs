using VelorenPort.CoreEngine;
using Xunit;

namespace CoreEngine.Tests;

public class RtSimRoleTests
{
    [Fact]
    public void CivilisedRole_StoresProfession()
    {
        var prof = new Profession.Adventurer(5);
        var role = new Role.Civilised(prof);
        Assert.IsType<Profession.Adventurer>(role.Profession);
        Assert.Equal<uint>(5, ((Profession.Adventurer)role.Profession!).Level);
    }

    [Fact]
    public void Response_CanAttachItem()
    {
        var resp = new Response(new Content.Plain("hi"));
        var item = new ItemDefinitionIdOwned.Simple("test");
        var with = resp.WithItem(item, 3);
        Assert.True(with.GivenItem.HasValue);
        Assert.Equal<uint>(3, with.GivenItem.Value.Amount);
    }
}
