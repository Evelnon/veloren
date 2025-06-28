using VelorenPort.World;
using VelorenPort.World.Sim;
using Unity.Mathematics;

namespace World.Tests;

public class SimUtilTests
{
    [Fact]
    public void MapEdgeFactor_CenterIsOne()
    {
        var size = new MapSizeLg(new int2(4,4));
        int centerIdx = size.Vec2AsUniformIdx(size.Chunks / 2);
        float f = Util.MapEdgeFactor(size, centerIdx);
        Assert.True(f > 0.9f);
    }

    [Fact]
    public void MapEdgeFactor_EdgeIsZero()
    {
        var size = new MapSizeLg(new int2(4,4));
        int idx = size.Vec2AsUniformIdx(int2.zero);
        float f = Util.MapEdgeFactor(size, idx);
        Assert.Equal(0f, f);
    }

    [Fact]
    public void CdfIrwinHall_TwoUniform()
    {
        float[] w = {1f,1f};
        float[] s = {0.5f,0.5f};
        float v = Util.CdfIrwinHall(w, s);
        Assert.True(math.abs(v - 0.5f) < 1e-4);
    }
}

