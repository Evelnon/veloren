using System.Linq;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.Terrain;
using VelorenPort.CoreEngine.Volumes;
using VelorenPort.NativeMath;

namespace CoreEngine.Tests;

public class BiomeVolumeTests
{
    [Fact]
    public void CompressedBiome_Conversions()
    {
        CompressedBiome c = BiomeKind.Forest;
        BiomeKind b = c;
        Assert.Equal(BiomeKind.Forest, b);
    }

    [Fact]
    public void RoundTrip_Compression()
    {
        var vol = new BiomeVolume(new int3(2,2,1), BiomeKind.Void);
        vol.Set(new int3(0,0,0), BiomeKind.Forest);
        vol.Set(new int3(1,0,0), BiomeKind.Desert);
        var shared = vol.ToShared();
        var back = BiomeVolume.FromShared(shared);
        var list1 = vol.Cells().Select(c => c.Biome).ToArray();
        var list2 = back.Cells().Select(c => c.Biome).ToArray();
        Assert.Equal(list1, list2);
    }
}
