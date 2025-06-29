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
        var dbPath = Path.Combine(dir, "chars.sqlite");
        var loader = new CharacterLoader(dbPath);
        loader.AddCharacter("p1", new CharacterId(1), "a");
        loader.AddCharacter("p1", new CharacterId(2), "b");

        var loaded = new CharacterLoader(dbPath);
        var list = loaded.LoadCharacterList("p1");
        Assert.Equal(2, list.Count);
        Assert.Contains(list, id => id.Value == 1);
        Directory.Delete(dir, true);
    }

    [Fact]
    public void LegacyFileMigrates()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var legacy = Path.Combine(dir, "character_list.json");
        File.WriteAllText(legacy, "{\"p2\":[3,4]}");
        var dbPath = Path.Combine(dir, "chars.sqlite");
        var loader = new CharacterLoader(dbPath);
        var list = loader.LoadCharacterList("p2");
        Assert.Equal(2, list.Count);
        Assert.Contains(list, id => id.Value == 3);
        Assert.True(File.Exists(legacy + ".bak"));
        Directory.Delete(dir, true);
    }
}
