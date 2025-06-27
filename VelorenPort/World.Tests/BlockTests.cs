using VelorenPort.World;

namespace World.Tests;

public class BlockTests
{
    [Fact]
    public void AirBlock_IsNotFilled()
    {
        var air = Block.Air;
        Assert.False(air.IsFilled);
        Assert.Null(air.GetColor());
    }

    [Fact]
    public void WithDataOf_PreservesKind()
    {
        var baseBlock = Block.Filled(BlockKind.Rock, 1, 2, 3);
        var other = Block.Filled(BlockKind.Rock, 4, 5, 6);
        var combined = baseBlock.WithDataOf(other);
        Assert.Equal(BlockKind.Rock, combined.Kind);
        Assert.Equal(other.Data, combined.Data);
    }
}
