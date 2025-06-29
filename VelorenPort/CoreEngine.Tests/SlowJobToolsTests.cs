using VelorenPort.CoreEngine.SlowJob;

namespace CoreEngine.Tests;

public class SlowJobToolsTests
{
    [Fact]
    public void Constant_HasExpectedValue()
    {
        Assert.Equal("CHUNK_GENERATOR", SlowJobTools.ChunkGenerator);
    }
}
