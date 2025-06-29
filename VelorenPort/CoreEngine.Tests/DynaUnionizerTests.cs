using Unity.Mathematics;
using VelorenPort.CoreEngine;
using VelorenPort.CoreEngine.figure;
using Xunit;

namespace CoreEngine.Tests;

public class DynaUnionizerTests
{
    private static bool Filled(MatCell c) => c.IsFilled;
    private static MatCell Empty() => MatCell.None;

    [Fact]
    public void UnifyMergesVolumesAndOffsets()
    {
        var a = Dyna<MatCell, object>.Filled(new int3(1,1,1), MatCell.FromMaterial(Material.Skin), new object());
        var b = Dyna<MatCell, object>.Filled(new int3(1,1,1), MatCell.FromMaterial(Material.Hair), new object());
        var union = new DynaUnionizer<MatCell>(Filled, Empty)
            .Add(a, int3.zero)
            .Add(b, new int3(1,0,0));
        var (vol, origin) = union.Unify();
        Assert.Equal(new int3(0,0,0), origin);
        Assert.Equal(new int3(2,1,1), vol.Size);
        Assert.Equal(Material.Skin, vol.Get(new int3(0,0,0)).Material);
        Assert.Equal(Material.Hair, vol.Get(new int3(1,0,0)).Material);
    }

    [Fact]
    public void UnifyHandlesNegativeOffsets()
    {
        var a = Dyna<MatCell, object>.Filled(new int3(1,1,1), MatCell.FromMaterial(Material.Skin), new object());
        var b = Dyna<MatCell, object>.Filled(new int3(1,1,1), MatCell.FromMaterial(Material.Hair), new object());
        var union = new DynaUnionizer<MatCell>(Filled, Empty)
            .Add(a, new int3(-1,0,0))
            .Add(b, int3.zero);
        var (vol, origin) = union.Unify();
        Assert.Equal(new int3(1,0,0), origin);
        Assert.Equal(new int3(2,1,1), vol.Size);
        Assert.Equal(Material.Skin, vol.Get(new int3(0,0,0)).Material);
        Assert.Equal(Material.Hair, vol.Get(new int3(1,0,0)).Material);
    }
}
