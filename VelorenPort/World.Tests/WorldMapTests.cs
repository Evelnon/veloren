using VelorenPort.World;
using Unity.Mathematics;

namespace World.Tests;

public class WorldMapTests
{
    [Fact]
    public void GetOrGenerateWithSupplement_CachesSupplement()
    {
        var map = new WorldMap();
        var noise = new Noise(1);
        var (chunk, sup) = map.GetOrGenerateWithSupplement(int2.zero, noise);
        Assert.NotNull(sup);
        var sup2 = map.GetSupplement(int2.zero);
        Assert.Same(sup, sup2);
        var (chunk2, sup3) = map.GetOrGenerateWithSupplement(int2.zero, noise);
        Assert.Same(chunk, chunk2);
        Assert.Same(sup, sup3);
    }
}
