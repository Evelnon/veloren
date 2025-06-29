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

    [Fact]
    public void WildlifeLayer_SpawnsNearResources()
    {
        var ctx = new LayerContext { ChunkPos = int2.zero, Noise = new Noise(2) };
        var chunk = new Chunk(int2.zero, Block.Earth);
        LayerManager.Apply(LayerType.Resource, ctx, chunk);
        int deposits = ctx.Supplement.ResourceDeposits.Count;
        Assert.True(deposits > 0);
        LayerManager.Apply(LayerType.Wildlife, ctx, chunk);
        Assert.True(chunk.Wildlife.Count >= deposits);
    }

    [Fact]
    public void ResourceDeposits_PersistBetweenGenerations()
    {
        var noise = new Noise(3);
        var (chunk1, sup1) = TerrainGenerator.GenerateChunkWithSupplement(int2.zero, noise);
        var (chunk2, sup2) = TerrainGenerator.GenerateChunkWithSupplement(int2.zero, noise);
        Assert.Equal(sup1.ResourceDeposits.Count, sup2.ResourceDeposits.Count);
    }

    [Fact]
    public void WorldSim_TickUpdatesResourceAmounts()
    {
        var index = new WorldIndex(0);
        var sim = new WorldSim(0, new int2(1, 1));
        var (chunk, sup) = index.Map.GetOrGenerateWithSupplement(int2.zero, index.Noise);
        sup.ResourceDeposits.Clear();
        var dep = new ResourceDeposit(new int3(0, 0, 0), BlockKind.GlowingRock, 1f);
        sup.ResourceDeposits.Add(dep);
        sim.Tick(index, 1f);
        Assert.True(sup.ResourceDeposits[0].Amount > 1f);

        sup.WildlifeEntities.Add(new WildlifeEntity(new int3(0, 0, 0), FaunaKind.Rabbit));
        float before = sup.ResourceDeposits[0].Amount;
        sim.Tick(index, 1f);
        Assert.True(sup.ResourceDeposits[0].Amount < before);
    }
}
