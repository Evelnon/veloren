using System;
using System.IO;
using System.Linq;
using VelorenPort.NativeMath;
using VelorenPort.Server;
using VelorenPort.World;

namespace Server.Tests;

public class TerrainRotationIntegrationTests
{
    [Fact]
    public void SavesRotateWhenLimitExceeded()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        var tp = new TerrainPersistence(dir, rotationLimit: 2);
        var key = new int2(0, 0);
        var pos = new int3(1, 1, 0);

        for (int i = 0; i < 3; i++)
        {
            tp.SetBlock(pos, Block.Filled(BlockKind.Rock, 1, 1, 1));
            tp.UnloadChunk(key);
        }

        var files = Directory.GetFiles(Path.Combine(dir, "terrain"), "chunk_0_0.dat*");
        Assert.Contains(Path.Combine(dir, "terrain", "chunk_0_0.dat"), files);
        Assert.Contains(Path.Combine(dir, "terrain", "chunk_0_0.dat.1"), files);
        Assert.Contains(Path.Combine(dir, "terrain", "chunk_0_0.dat.2"), files);
        Assert.Equal(3, files.Length);
        Directory.Delete(dir, true);
    }
}
