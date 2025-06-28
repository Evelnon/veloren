using VelorenPort.World.Site.Util;
using Unity.Mathematics;

namespace World.Tests;

public class DirTests
{
    [Fact]
    public void ToMat3_RotatesCorrectly()
    {
        float3 v = math.mul(Dir.NegX.ToMat3(), new float3(1, 0, 0));
        Assert.Equal(new float3(-1, 0, 0), v);
    }

    [Fact]
    public void RotateAxisCw_RotatesAroundZ()
    {
        var dir = Dir3.X.RotateAxisCw(Dir3.Z);
        Assert.Equal(Dir3.Y, dir);
    }
}
