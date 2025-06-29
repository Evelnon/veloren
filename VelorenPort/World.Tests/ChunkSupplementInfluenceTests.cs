using VelorenPort.World;
using VelorenPort.NativeMath;

namespace World.Tests;

public class ChunkSupplementInfluenceTests
{
    [Fact]
    public void GenerateChunkWithSupplement_UsesWildlifeFromSupplement()
    {
        var sup = new ChunkSupplement();
        var spawn = new FaunaSpawn(new int3(2,2,1), FaunaKind.Wolf);
        sup.Wildlife.Add(spawn);
        var (chunk, _) = TerrainGenerator.GenerateChunkWithSupplement(int2.zero, new Noise(0), sup);
        Assert.Contains(spawn, chunk.Wildlife);
    }

    [Fact]
    public void GenerateChunkWithSupplement_HonorsDepletedResources()
    {
        var noise = new Noise(1);
        var (baseChunk, baseSup) = TerrainGenerator.GenerateChunkWithSupplement(int2.zero, noise);
        Assert.NotEmpty(baseSup.ResourceDeposits);
        var dep = baseSup.ResourceDeposits[0];
        dep.MarkDepleted();
        baseSup.ResourceDeposits[0] = dep;
        var (chunk, _) = TerrainGenerator.GenerateChunkWithSupplement(int2.zero, noise, baseSup);
        Assert.Equal(BlockKind.Air, chunk[dep.Position.x, dep.Position.y, dep.Position.z].Kind);
    }
}
