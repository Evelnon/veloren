using VelorenPort.World.Site.Util;
using VelorenPort.NativeMath;

namespace World.Tests;

public class TileSpriteTests
{
    [Fact]
    public void TwoBy_SetsCorrectBounds()
    {
        var t = Tileable2.TwoBy(3, new int3(0,0,0), Dir.X);
        Assert.Equal(new int2(-1,-1), t.Bounds.Min);
        Assert.Equal(new int2(1,0), t.Bounds.Max);
    }

    [Fact]
    public void Rotation_AffectsSideLookup()
    {
        var t = Tileable2.Empty().WithRotation(Dir.Y);
        var block = new VelorenPort.World.Block();
        t = t.WithSide(Dir.X, block);
        Assert.Equal(block, t.Side(Dir.NegY));
    }
}
