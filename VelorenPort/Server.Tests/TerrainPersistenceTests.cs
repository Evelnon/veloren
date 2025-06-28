using System;
using System.IO;
using Unity.Mathematics;
using VelorenPort.Server;
using VelorenPort.World;

namespace Server.Tests;

public class TerrainPersistenceTests {
    [Fact]
    public void SetBlock_Unload_Reload_PreservesChange() {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var tp = new TerrainPersistence(dir);
        var worldPos = new int3(1, 1, 0);
        tp.SetBlock(worldPos, Block.Filled(BlockKind.Rock, 1,2,3));
        tp.UnloadChunk(new int2(0,0));

        var chunk = new Chunk(new int2(0,0), Block.Air);
        tp.ApplyChanges(new int2(0,0), chunk);
        Assert.Equal(BlockKind.Rock, chunk[1,1,0].Kind);
        Directory.Delete(dir, true);
    }
}
