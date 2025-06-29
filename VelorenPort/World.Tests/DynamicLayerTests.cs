using VelorenPort.World.Layer;
using VelorenPort.World;
using Unity.Mathematics;

namespace World.Tests;

public class DynamicLayerTests
{
    [Fact]
    public void Apply_DoesNotThrow()
    {
        var ctx = new LayerContext { ChunkPos = new int2(0, 0) };
        var chunk = new Chunk(int2.zero, Block.Air);
        LayerManager.Apply(LayerType.Cave, ctx, chunk);
        LayerManager.Apply(LayerType.Tree, ctx, chunk);
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
}
