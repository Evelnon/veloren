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
    public void Scatter_AddsSpotWhenProbabilityHigh()
    {
        var ctx = new LayerContext
        {
            ChunkPos = new int2(0, 0),
            ScatterChance = 1.0,
            Rng = new System.Random(0)
        };
        LayerManager.Apply(LayerType.Scatter, ctx);
        Assert.Single(ctx.Supplement.Entities);
    }
}
