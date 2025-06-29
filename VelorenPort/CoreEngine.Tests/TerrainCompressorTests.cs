using VelorenPort.CoreEngine.Terrain;

namespace CoreEngine.Tests;

public class TerrainCompressorTests
{
    [Fact]
    public void CompressAndDecompress_RoundTrip()
    {
        int[] data = {1,1,1,2,2,3};
        var comp = TerrainCompressor.Compress(data);
        var decomp = TerrainCompressor.Decompress(comp);
        Assert.Equal(data, decomp.ToArray());
    }
}
