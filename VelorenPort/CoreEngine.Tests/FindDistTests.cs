using VelorenPort.CoreEngine;
using VelorenPort.NativeMath;

namespace CoreEngine.Tests;

public class FindDistTests
{
    [Fact]
    public void CylinderCube_DistanceSymmetry()
    {
        var cyl = new FindDist.Cylinder(new float3(0f,0f,0f),2f,4f);
        var cube = new FindDist.Cube(new float3(-0.5f,-0.5f,-0.5f),1f);
        Assert.True(FindDist.ApproxInRange(cube, cyl, 0f));
        float d1 = FindDist.MinDistance(cube, cyl);
        float d2 = FindDist.MinDistance(cyl, cube);
        Assert.True(math.abs(d1) < 1e-5f);
        Assert.True(math.abs(d1 - d2) < 1e-3f);
    }

    [Fact]
    public void CylinderPoint_Distance()
    {
        var cyl = new FindDist.Cylinder(new float3(1f,2f,3f),0f,0f);
        float3 p = new float3(1f,2.5f,3.5f);
        Assert.True(FindDist.ApproxInRange(cyl, p, 0.71f));
        float d = FindDist.MinDistance(cyl, p);
        Assert.True(d < 0.71f && d > 0.70f);
    }
}
