using VelorenPort.World;
using VelorenPort.World.Sim;
using VelorenPort.NativeMath;

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

    [Fact]
    public void GetOceans_AllOcean()
    {
        var size = new MapSizeLg(new int2(2, 2));
        bool[] res = Util.GetOceans(size, _ => -1f);
        foreach (bool b in res)
            Assert.True(b);
    }

    [Fact]
    public void GetOceans_InteriorLand()
    {
        var size = new MapSizeLg(new int2(2, 2));
        int2 chunks = size.Chunks;
        bool[] res = Util.GetOceans(size, idx =>
        {
            int2 pos = size.UniformIdxAsVec2(idx);
            bool edge = pos.x == 0 || pos.y == 0 || pos.x == chunks.x - 1 || pos.y == chunks.y - 1;
            return edge ? 0f : 10f;
        });

        int interiorIdx = size.Vec2AsUniformIdx(new int2(1, 1));
        Assert.False(res[interiorIdx]);
    }

    [Fact]
    public void Downhill_FindsLowestNeighbor()
    {
        var size = new MapSizeLg(new int2(1, 1));
        float[] alt = {4f, 3f, 2f, 1f};
        int[] dh = Util.Downhill(size, idx => alt[idx], _ => false);
        Assert.Equal(3, dh[0]);
        Assert.Equal(3, dh[1]);
        Assert.Equal(3, dh[2]);
        Assert.Equal(-1, dh[3]);
    }

    [Fact]
    public void LocalCells_CenterHasFullNeighborhood()
    {
        var size = new MapSizeLg(new int2(4, 4));
        int idx = size.Vec2AsUniformIdx(new int2(8, 8));
        int count = Util.LocalCells(size, idx).Count();
        Assert.Equal(49, count);
    }

    [Fact]
    public void LocalCells_EdgeReduced()
    {
        var size = new MapSizeLg(new int2(4, 4));
        int idx = size.Vec2AsUniformIdx(int2.zero);
        int count = Util.LocalCells(size, idx).Count();
        Assert.Equal(16, count);
    }

    [Fact]
    public void Uphill_ReturnsUpstreamNeighbors()
    {
        var size = new MapSizeLg(new int2(1, 1));
        float[] alt = {4f, 3f, 2f, 1f};
        int[] dh = Util.Downhill(size, idx => alt[idx], _ => false);
        var up = Util.Uphill(size, dh, 3).ToArray();
        Assert.Contains(0, up);
        Assert.Contains(1, up);
        Assert.Contains(2, up);
        Assert.Equal(3, up.Length);
    }

    [Fact]
    public void UniformNoise_RanksValues()
    {
        var size = new MapSizeLg(new int2(1,1));
        float[] vals = {4f,1f,3f,2f};
        var (uniform, noise) = Util.UniformNoise(size, (i, _) => vals[i]);
        Assert.Equal((1f/4f,1f), uniform[1]);
        Assert.Equal((2f/4f,2f), uniform[3]);
        Assert.Equal((3f/4f,3f), uniform[2]);
        Assert.Equal((4f/4f,4f), uniform[0]);
        Assert.Equal(4, noise.Length);
    }
}

