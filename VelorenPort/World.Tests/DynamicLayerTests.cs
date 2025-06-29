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
}
