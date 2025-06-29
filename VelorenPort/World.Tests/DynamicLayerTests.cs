using VelorenPort.World.Layer;
using VelorenPort.World;
using VelorenPort.NativeMath;

namespace World.Tests;

public class DynamicLayerTests
{
    [Fact]
    public void Apply_DoesNotThrow()
    {
        var ctx = new LayerContext { ChunkPos = new int2(0, 0) };
        var chunk = new Chunk(int2.zero, Block.Air);
        LayerManager.Apply(LayerType.Cave, ctx, chunk);
        LayerManager.Apply(LayerType.Vegetation, ctx, chunk);
        LayerManager.Apply(LayerType.Wildlife, ctx, chunk);
    }

    [Fact]
    public void ResourceLayer_AddsOre()
    {
        var ctx = new LayerContext { ChunkPos = int2.zero, Noise = new Noise(1) };
        var chunk = new Chunk(int2.zero, Block.Earth);
        LayerManager.Apply(LayerType.Resource, ctx, chunk);
        bool found = false;
        for (int x = 0; x < Chunk.Size.x && !found; x++)
        for (int y = 0; y < Chunk.Size.y && !found; y++)
        for (int z = 1; z < Chunk.Height / 2 && !found; z++)
            if (chunk[x,y,z].Kind == BlockKind.GlowingRock || chunk[x,y,z].Kind == BlockKind.GlowingWeakRock)
                found = true;
        Assert.True(found);
    }

    [Fact]
    public void Spot_AddsSpotWhenProbabilityHigh()
    {
        var ctx = new LayerContext
        {
            ChunkPos = new int2(0, 0),
            ScatterChance = 1.0,
            Rng = new System.Random(0)
        };
        LayerManager.Apply(LayerType.Spot, ctx);
        Assert.Single(ctx.Supplement.Entities);
    }

    [Fact]
    public void VegetationLayer_AddsBlocks()
    {
        var ctx = new LayerContext { ChunkPos = int2.zero, Noise = new Noise(2) };
        var chunk = new Chunk(int2.zero, Block.Earth);
        LayerManager.Apply(LayerType.Vegetation, ctx, chunk);
        bool found = false;
        for (int x = 0; x < Chunk.Size.x && !found; x++)
        for (int y = 0; y < Chunk.Size.y && !found; y++)
        for (int z = 0; z < Chunk.Height && !found; z++)
            if (chunk[x,y,z].Kind == BlockKind.Wood || chunk[x,y,z].Kind == BlockKind.Leaves)
                found = true;
        Assert.True(found);
    }

    [Fact]
    public void RockLayer_AddsWeakRock()
    {
        var ctx = new LayerContext { ChunkPos = int2.zero, Noise = new Noise(3) };
        var chunk = new Chunk(int2.zero, Block.Earth);
        LayerManager.Apply(LayerType.Rock, ctx, chunk);
        bool found = false;
        for (int x = 0; x < Chunk.Size.x && !found; x++)
        for (int y = 0; y < Chunk.Size.y && !found; y++)
        for (int z = 0; z < Chunk.Height && !found; z++)
            if (chunk[x,y,z].Kind == BlockKind.WeakRock)
                found = true;
        Assert.True(found);
    }
}
