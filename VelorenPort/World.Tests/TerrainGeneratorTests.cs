using Unity.Mathematics;
using VelorenPort.World;

namespace World.Tests;

public class TerrainGeneratorTests
{
    [Fact]
    public void GenerateChunk_ProducesEarthBlocks()
    {
        var chunk = TerrainGenerator.GenerateChunk(new int2(0, 0), new Noise(0));
        Assert.Equal(BlockKind.Earth, chunk[0,0,0].Kind);
    }

    [Fact]
    public void WorldMap_GetOrGenerate_ReturnsSameInstance()
    {
        var map = new WorldMap();
        var noise = new Noise(1);
        var first = map.GetOrGenerate(new int2(1,1), noise);
        var second = map.GetOrGenerate(new int2(1,1), noise);
        Assert.Same(first, second);
    }

    [Fact]
    public void Block_RoundTripPreservesData()
    {
        var block = Block.Filled(BlockKind.Rock, 1, 2, 3);
        uint raw = block.ToUInt32();
        var from = Block.FromUInt32(raw);
        Assert.Equal(block.Kind, from.Kind);
        Assert.Equal(block.Data, from.Data);
    }

    [Fact]
    public void GenerateChunkWithSupplement_ReturnsResourceInfo()
    {
        var (chunk, sup) = TerrainGenerator.GenerateChunkWithSupplement(new int2(0,0), new Noise(0));
        int resourceCount = 0;
        for (int z = 0; z < Chunk.Height; z++)
        for (int y = 0; y < Chunk.Size.y; y++)
        for (int x = 0; x < Chunk.Size.x; x++)
            if (chunk[x,y,z].GetRtsimResource() != null)
                resourceCount++;

        Assert.Equal(resourceCount, sup.ResourceBlocks.Count);
        Assert.Equal(chunk.Wildlife.Count, sup.Wildlife.Count);
        Assert.True(resourceCount > 0);
    }
}
