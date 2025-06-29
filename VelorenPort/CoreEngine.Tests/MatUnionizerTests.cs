using VelorenPort.CoreEngine.figure;
using VelorenPort.NativeMath;

namespace CoreEngine.Tests;

public class MatUnionizerTests
{
    [Fact]
    public void Union_MergesSegments()
    {
        var seg1 = Segment.Filled(new int3(1,1,1), MatCell.FromMaterial(Material.Skin));
        var seg2 = Segment.Filled(new int3(1,1,1), MatCell.FromMaterial(Material.Hair));
        var result = MatUnionizer.Union(new[]{(seg1, new int3(0,0,0)), (seg2, new int3(1,0,0))});
        Assert.Equal(new int3(2,1,1), result.Size);
        Assert.True(result.Get(new int3(1,0,0)).IsMaterial);
    }
}
