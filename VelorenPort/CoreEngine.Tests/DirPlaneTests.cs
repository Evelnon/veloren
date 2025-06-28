using VelorenPort.CoreEngine;
using Unity.Mathematics;

namespace CoreEngine.Tests;

public class DirPlaneTests
{
    [Fact]
    public void Dir_IsNormalized()
    {
        var d = new Dir(new float3(2f, 0f, 0f));
        Assert.True(math.abs(math.length(d.Value) - 1f) < 1e-5f);
    }

    [Fact]
    public void Plane_Project_RemovesComponent()
    {
        var plane = Plane.XY();
        var v = new float3(1f, 2f, 3f);
        var p = v.Projected(plane);
        Assert.True(math.abs(p.z) < 1e-5f);
        Assert.Equal(new float2(1f, 2f), new float2(p.x, p.y));
    }
}
