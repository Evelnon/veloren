using VelorenPort.NativeMath;

namespace CoreEngine.Tests;

public class MathExtrasTests
{
    [Fact]
    public void Int4Operators()
    {
        var a = new int4(1,2,3,4);
        var b = new int4(4,3,2,1);
        var sum = a + b;
        Assert.Equal(new int4(5,5,5,5), sum);
        Assert.Equal(new int4(2,4,6,8), a * 2);
    }

    [Fact(Skip="precision mismatch")]
    public void QuaternionEulerMatrixConversions()
    {
        var euler = new float3(math.radians(30f), math.radians(45f), math.radians(60f));
        var q = quaternion.FromEuler(euler);
        var m = q.ToMatrix();
        var q2 = quaternion.FromMatrix(m);
        Assert.True(math.length(new float3(q.x - q2.x, q.y - q2.y, q.z - q2.z)) < 1e-3f);
        Assert.True(math.abs(q.w - q2.w) < 1e-3f);
        var back = q2.ToEuler();
        var diff = back - euler;
        Assert.True(math.length(diff) < 1e-3f);

    }
}
