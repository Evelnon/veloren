using VelorenPort.World;
using Unity.Mathematics;

namespace World.Tests;

public class CanvasTests
{
    [Fact]
    public void FindSpawnPos_ReturnsValidPosition()
    {
        var chunk = new Chunk(int2.zero, Block.Air);
        for (int x = 0; x < Chunk.Size.x; x++)
        for (int y = 0; y < Chunk.Size.y; y++)
            chunk[x, y, 0] = Block.Filled(BlockKind.Rock, 1, 1, 1);

        var sim = new WorldSim(0, new int2(1,1));
        var cinfo = new CanvasInfo(int2.zero, sim, new SimChunk());
        var canvas = new Canvas(cinfo, chunk);

        var spawn = canvas.FindSpawnPos(new int3(cinfo.Wpos, 8));
        Assert.NotNull(spawn);
        Assert.Equal(1, spawn!.Value.z);
    }

    [Fact]
    public void Spawn_AddsEntryToList()
    {
        var chunk = new Chunk(int2.zero, Block.Air);
        var canvas = new Canvas(new CanvasInfo(int2.zero, new WorldSim(0, new int2(1,1)), new SimChunk()), chunk);
        canvas.Spawn(new int3(1,2,3));
        Assert.Single(canvas.Spawns);
        Assert.Equal(new int3(1,2,3), canvas.Spawns[0]);
    }
}

