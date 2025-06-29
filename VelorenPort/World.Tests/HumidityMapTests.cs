using Unity.Mathematics;
using VelorenPort.World;
using VelorenPort.World.Sim;
using Xunit;

namespace World.Tests;

public class HumidityMapTests
{
    [Fact]
    public void Generate_SetsInitialValue()
    {
        var (world, _) = World.Generate(0);
        var map = HumidityMap.Generate(world, 0.25f);
        Assert.Equal(0.25f, map.Get(new int2(0, 0)));
    }

    [Fact]
    public void Set_UpdatesValue()
    {
        var (world, _) = World.Generate(0);
        var map = HumidityMap.Generate(world);
        map.Set(new int2(0, 0), 0.8f);
        Assert.Equal(0.8f, map.Get(new int2(0, 0)));
    }

    [Fact]
    public void Diffuse_SpreadsHumidity()
    {
        var (world, _) = World.Generate(0);
        var map = HumidityMap.Generate(world, 0f);
        map.Set(new int2(1, 1), 1f);
        map.RunDiffusion(3, 0.5f);
        Assert.True(map.Get(new int2(0, 0)) > 0f);
        Assert.True(map.Get(new int2(1, 1)) < 1f);
    }

    [Fact]
    public void RunDiffusion_SpreadsAcrossWorld()
    {
        var (world, _) = World.Generate(0);
        var map = HumidityMap.Generate(world, 0f);
        map.Set(new int2(2, 2), 1f);
        map.RunDiffusion(8, 0.5f);
        foreach (var (_, val) in map.Iterate())
            Assert.True(val > 0f);
    }
}
