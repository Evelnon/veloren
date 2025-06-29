using VelorenPort.World;
using VelorenPort.World.Layer;
using Unity.Mathematics;

namespace World.Tests;

public class DynamicLayerTests
{
    [Fact]
    public void Apply_DoesNotThrow()
    {
        var ctx = new LayerContext { ChunkPos = new int2(0, 0) };
        LayerManager.Apply(LayerType.Cave, ctx);
        LayerManager.Apply(LayerType.Tree, ctx);
    }

    [Fact]
    public void TreeLayer_PlacesBlocks()
    {
        var chunk = new Chunk(int2.zero, Block.Air);
        var canvas = new Canvas(new CanvasInfo(int2.zero, new WorldSim(0, new int2(1,1)), new SimChunk()), chunk);
        var ctx = new LayerContext { ChunkPos = int2.zero, Canvas = canvas };
        LayerManager.Apply(LayerType.Tree, ctx);

        bool found = false;
        for (int x = 0; x < Chunk.Size.x && !found; x++)
        for (int y = 0; y < Chunk.Size.y && !found; y++)
        for (int z = 0; z < Chunk.Height && !found; z++)
            if (chunk[x,y,z].Kind == BlockKind.Wood || chunk[x,y,z].Kind == BlockKind.Leaves)
                found = true;

        Assert.True(found);
    }

    [Fact]
    public void ShrubLayer_PlacesBlocks()
    {
        var chunk = new Chunk(int2.zero, Block.Air);
        var canvas = new Canvas(new CanvasInfo(int2.zero, new WorldSim(0, new int2(1,1)), new SimChunk()), chunk);
        var ctx = new LayerContext { ChunkPos = int2.zero, Canvas = canvas };
        LayerManager.Apply(LayerType.Shrub, ctx);

        bool found = false;
        for (int x = 0; x < Chunk.Size.x && !found; x++)
        for (int y = 0; y < Chunk.Size.y && !found; y++)
        for (int z = 0; z < Chunk.Height && !found; z++)
            if (chunk[x,y,z].Kind == BlockKind.Leaves)
                found = true;

        Assert.True(found);
    }

    [Fact]
    public void CaveLayer_CarvesBlocks()
    {
        var chunk = new Chunk(int2.zero, Block.Filled(BlockKind.Rock, 1,1,1));
        var canvas = new Canvas(new CanvasInfo(int2.zero, new WorldSim(0, new int2(1,1)), new SimChunk()), chunk);
        var ctx = new LayerContext { ChunkPos = int2.zero, Canvas = canvas };
        LayerManager.Apply(LayerType.Cave, ctx);

        bool air = false;
        for (int x = 0; x < Chunk.Size.x && !air; x++)
        for (int y = 0; y < Chunk.Size.y && !air; y++)
        for (int z = 0; z < Chunk.Height && !air; z++)
            if (chunk[x,y,z].Kind == BlockKind.Air)
                air = true;

        Assert.True(air);
    }

    [Fact]
    public void ScatterLayer_PlacesRocks()
    {
        var chunk = new Chunk(int2.zero, Block.Air);
        for (int x = 0; x < Chunk.Size.x; x++)
        for (int y = 0; y < Chunk.Size.y; y++)
            chunk[x,y,0] = Block.Filled(BlockKind.Earth, 1,1,1);

        var canvas = new Canvas(new CanvasInfo(int2.zero, new WorldSim(0, new int2(1,1)), new SimChunk()), chunk);
        var ctx = new LayerContext { ChunkPos = int2.zero, Canvas = canvas };
        LayerManager.Apply(LayerType.Scatter, ctx);

        bool rock = false;
        for (int x = 0; x < Chunk.Size.x && !rock; x++)
        for (int y = 0; y < Chunk.Size.y && !rock; y++)
        for (int z = 0; z < Chunk.Height && !rock; z++)
            if (chunk[x,y,z].Kind == BlockKind.Rock)
                rock = true;

        Assert.True(rock);
    }

    [Fact]
    public void WildlifeLayer_AddsSpawns()
    {
        var chunk = new Chunk(int2.zero, Block.Air);
        for (int x = 0; x < Chunk.Size.x; x++)
        for (int y = 0; y < Chunk.Size.y; y++)
            chunk[x,y,0] = Block.Filled(BlockKind.Earth,1,1,1);

        var canvas = new Canvas(new CanvasInfo(int2.zero, new WorldSim(0, new int2(1,1)), new SimChunk()), chunk);
        var ctx = new LayerContext { ChunkPos = int2.zero, Canvas = canvas };
        LayerManager.Apply(LayerType.Wildlife, ctx);

        Assert.NotEmpty(canvas.Spawns);
    }

    [Fact]
    public void OreVeinLayer_PlacesGlowingRock()
    {
        var chunk = new Chunk(int2.zero, Block.Filled(BlockKind.Rock,1,1,1));
        var canvas = new Canvas(new CanvasInfo(int2.zero, new WorldSim(0, new int2(1,1)), new SimChunk()), chunk);
        var ctx = new LayerContext { ChunkPos = int2.zero, Canvas = canvas };
        LayerManager.Apply(LayerType.OreVein, ctx);

        bool ore = false;
        for (int x = 0; x < Chunk.Size.x && !ore; x++)
        for (int y = 0; y < Chunk.Size.y && !ore; y++)
        for (int z = 0; z < Chunk.Height && !ore; z++)
            if (chunk[x,y,z].Kind == BlockKind.GlowingRock)
                ore = true;

        Assert.True(ore);
        Assert.NotEmpty(canvas.ResourceBlocks);
    }
}
