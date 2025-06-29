using System;
using VelorenPort.World.Util;
using Xunit;

namespace World.Tests;

public class MapArrayTests
{
    private enum TestEnum { A, B, C }

    [Fact]
    public void Indexing_Works()
    {
        var map = new MapArray<TestEnum, int>(0);
        map[TestEnum.B] = 5;
        Assert.Equal(5, map[TestEnum.B]);
        Assert.Equal(0, map[TestEnum.C]);
    }

    [Fact]
    public void EnumHelpers_Work()
    {
        int idx = EnumIndex.IndexFromEnum(TestEnum.C);
        Assert.Equal(2, idx);
        Assert.Equal(TestEnum.B, EnumIndex.EnumFromIndex<TestEnum>(1));
    }
}
