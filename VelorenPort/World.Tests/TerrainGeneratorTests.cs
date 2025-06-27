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
}
