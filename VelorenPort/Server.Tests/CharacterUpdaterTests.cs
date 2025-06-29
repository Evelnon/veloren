using System;
using System.IO;
using VelorenPort.Server;
using Unity.Entities;

namespace Server.Tests;

public class CharacterUpdaterTests {
    [Fact]
    public void SaveAndLoad_RoundTrip() {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "characters.json");
        var updater = new CharacterUpdater(path);
        var id = updater.CreateCharacter(Entity.Null, "player", "alias", null!);
        updater.SaveAll();

        var loaded = new CharacterUpdater(path);
        loaded.LoadAll();

        Assert.Equal(updater.Characters.Count, loaded.Characters.Count);
        Assert.True(loaded.Characters.ContainsKey(id));
        Directory.Delete(dir, true);
    }
}
