using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class AabbTests
{
    [Fact]
    public void Contains_And_Clamp_WorkCorrectly()
    {
        var box = new Aabb(new int3(0,0,0), new int3(10,10,10));
        Assert.True(box.Contains(new int3(5,5,5)));
        Assert.False(box.Contains(new int3(10,0,0)));
        Assert.Equal(new int3(0,0,0), box.Clamp(new int3(-1,-2,0)));
    }

    [Fact]
    public void Union_And_Translate_CreateExpectedBox()
    {
        var a = new Aabb(new int3(0,0,0), new int3(3,3,3));
        var b = new Aabb(new int3(2,2,2), new int3(5,4,4));
        var u = a.Union(b);
        Assert.Equal(new int3(0,0,0), u.Min);
        Assert.Equal(new int3(5,4,4), u.Max);

        var t = u.Translate(new int3(1,1,1));
        Assert.Equal(new int3(1,1,1), t.Min);
        Assert.Equal(new int3(6,5,5), t.Max);
    }

    [Fact]
    public void Intersection_ReturnsOverlap()
    {
        var a = new Aabb(new int3(0,0,0), new int3(5,5,5));
        var b = new Aabb(new int3(3,2,1), new int3(6,7,4));
        var i = a.Intersection(b);
        Assert.Equal(new int3(3,2,1), i.Min);
        Assert.Equal(new int3(5,5,4), i.Max);
    }

    [Fact]
    public void Scale_MultipliesBounds()
    {
        var box = new Aabb(new int3(1,2,3), new int3(4,5,6));
        var s = box.Scale(2);
        Assert.Equal(new int3(2,4,6), s.Min);
        Assert.Equal(new int3(8,10,12), s.Max);
    }

    [Fact]
    public void Rotate_ComputesAxisAlignedBounds()
    {
        var box = new Aabb(new int3(0,0,0), new int3(1,2,3));
        var rot = math.axisAngle(new float3(0f,0f,1f), math.PI * 0.5f);
        var r = box.Rotate(rot);
        Assert.Equal(new int3(-2,0,0), r.Min);
        Assert.Equal(new int3(0,1,3), r.Max);
    }
}
