using VelorenPort.World;
using VelorenPort.NativeMath;

namespace World.Tests;

public class WildlifeBehaviourTests
{
    [Fact]
    public void WildlifeEntity_StateTransitions()
    {
        var entity = new WildlifeEntity(int3.zero, FaunaKind.Wolf);
        entity.Tick(1.1f);
        Assert.Equal(FaunaBehaviourState.Roaming, entity.State);
        entity.Tick(60f);
        Assert.Equal(FaunaBehaviourState.Despawn, entity.State);
    }

    [Fact]
    public void ResourceLayer_WritesDepositsToSupplement()
    {
        var ctx = new LayerContext { ChunkPos = int2.zero, Noise = new Noise(0) };
        var chunk = new Chunk(int2.zero, Block.Earth);
        LayerManager.Apply(LayerType.Resource, ctx, chunk);
        Assert.Equal(ctx.Supplement.ResourceBlocks.Count, ctx.Supplement.ResourceDeposits.Count);
        Assert.True(ctx.Supplement.ResourceBlocks.Count > 0);
    }

    [Fact]
    public void GenerateChunkWithSupplement_StoresWildlifeEntities()
    {
        var (chunk, sup) = TerrainGenerator.GenerateChunkWithSupplement(int2.zero, new Noise(1));
        Assert.Equal(sup.Wildlife.Count, sup.WildlifeEntities.Count);
        Assert.Equal(sup.ResourceBlocks.Count, sup.ResourceDeposits.Count);
    }
}
