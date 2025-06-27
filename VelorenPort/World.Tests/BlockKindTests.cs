using VelorenPort.CoreEngine;
using VelorenPort.World;

namespace World.Tests;

public class BlockKindTests
{
    [Fact]
    public void ExtensionMethods_WorkAsExpected()
    {
        Assert.True(BlockKind.Air.IsAir());
        Assert.False(BlockKind.Water.IsAir());
        Assert.True(BlockKind.Water.IsLiquid());
        Assert.Equal(LiquidKind.Water, BlockKind.Water.LiquidKind());
        Assert.True(BlockKind.Rock.IsFilled());
        Assert.True(BlockKind.Rock.HasColor());
        Assert.True(BlockKind.Grass.IsTerrain());
    }
}
