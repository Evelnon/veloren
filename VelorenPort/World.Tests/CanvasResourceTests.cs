using VelorenPort.World;
using Unity.Mathematics;
using Xunit;

namespace World.Tests;

public class CanvasResourceTests
{
    [Fact]
    public void WriteSupplementData_CopiesResourceBlocks()
    {
        var chunk = new Chunk(int2.zero, Block.Air);
        var canvas = new Canvas(new CanvasInfo(int2.zero, new WorldSim(0, new int2(1,1)), new SimChunk()), chunk);
        canvas.SetBlock(new int3(1,1,0), new Block(BlockKind.Wood));
        var sup = new ChunkSupplement();
        canvas.WriteSupplementData(sup);
        Assert.Single(sup.ResourceBlocks);
        Assert.Equal(new int3(1,1,0), sup.ResourceBlocks[0]);
    }

    [Fact]
    public void WriteSupplementData_CopiesWildlifeSpawns()
    {
        var chunk = new Chunk(int2.zero, Block.Air);
        var canvas = new Canvas(new CanvasInfo(int2.zero, new WorldSim(0, new int2(1,1)), new SimChunk()), chunk);
        canvas.Spawn(new int3(2,2,1), FaunaKind.Wolf);
        var sup = new ChunkSupplement();
        canvas.WriteSupplementData(sup);
        Assert.Single(sup.Wildlife);
        Assert.Equal(FaunaKind.Wolf, sup.Wildlife[0].Kind);
        Assert.Equal(new int3(2,2,1), sup.Wildlife[0].Position);
    }
}
