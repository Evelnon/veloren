using System;
using System.IO;
using VelorenPort.Server.Persistence;
using VelorenPort.CoreEngine;

namespace Server.Tests;

public class CharacterLoaderTests
{
    [Fact]
    public void SaveAndLoad_RoundTrip()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "list.json");
        var loader = new CharacterLoader(path);
        loader.AddCharacter("p1", new CharacterId(1));
        loader.AddCharacter("p1", new CharacterId(2));
        loader.SaveAll();

        var loaded = new CharacterLoader(path);
        loaded.LoadAll();
        var list = loaded.LoadCharacterList("p1");
        Assert.Equal(2, list.Count);
        Assert.Contains(list, id => id.Value == 1);
        Directory.Delete(dir, true);
    }
}
