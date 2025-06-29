using VelorenPort.NativeMath;
using VelorenPort.World;
using VelorenPort.World.Sim;
using System.IO;
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
        map.Diffuse(1f, steps: 2);
        Assert.True(map.Get(new int2(0, 0)) > 0f);
        Assert.True(map.Get(new int2(1, 1)) < 1f);
    }

    [Fact]
    public void SaveAndLoad_RoundTrips()
    {
        var (world, _) = World.Generate(0);
        var map = HumidityMap.Generate(world, 0.3f);
        map.Set(new int2(1, 0), 0.6f);
        var path = Path.GetTempFileName();
        map.Save(path);
        var loaded = HumidityMap.Load(path);
        Assert.Equal(map.Size, loaded.Size);
        Assert.Equal(map.Get(new int2(1, 0)), loaded.Get(new int2(1, 0)));
        File.Delete(path);
    }
}
