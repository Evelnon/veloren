using Unity.Mathematics;
using VelorenPort.CoreEngine.States;

namespace CoreEngine.Tests;

public class ForcedMovementTests
{
    [Fact]
    public void Multiplication_ScalesValues()
    {
        ForcedMovement fm = new ForcedMovement.Leap(1f, 2f, 0.5f, MovementDirection.Look);
        var scaled = fm.Mul(2f) as ForcedMovement.Leap;
        Assert.NotNull(scaled);
        Assert.Equal(2f, scaled!.Vertical);
        Assert.Equal(4f, scaled.Fwd);
        Assert.Equal(0.5f, scaled.Progress);
        Assert.Equal(MovementDirection.Look, scaled.Direction);
    }

    [Fact]
    public void Get2dDir_NormalizesVectors()
    {
        float2 look = new float2(2, 0);
        float2 move = new float2(0, 3);
        float2 res1 = MovementDirection.Look.Get2dDir(look, move);
        float2 res2 = MovementDirection.Move.Get2dDir(look, move);
        Assert.Equal(new float2(1, 0), res1);
        Assert.Equal(new float2(0, 1), res2);
    }
}
