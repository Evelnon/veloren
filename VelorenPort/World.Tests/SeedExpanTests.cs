using VelorenPort.World.Util;
using Xunit;

namespace World.Tests;

public class SeedExpanTests
{
    [Fact]
    public void Diffuse_ProducesExpectedValue()
    {
        uint val = SeedExpan.Diffuse(123456789u);
        Assert.Equal(0x2D198CABu, val);
    }

    [Fact]
    public void DiffuseMult_CombinesValues()
    {
        uint val = SeedExpan.DiffuseMult(new uint[]{1u,2u,3u});
        Assert.Equal(0x5F9A860Au, val);
    }

    [Fact]
    public void RngState_Generates32Bytes()
    {
        byte[] state = SeedExpan.RngState(123u);
        Assert.Equal(32, state.Length);
        Assert.Equal(0x53, state[0]);
        Assert.Equal(0x97, state[1]);
    }
}
