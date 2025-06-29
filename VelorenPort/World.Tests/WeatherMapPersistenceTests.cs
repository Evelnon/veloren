using System.IO;
using VelorenPort.World.Sim;
using VelorenPort.NativeMath;

namespace World.Tests;

public class WeatherMapPersistenceTests
{
    [Fact]
    public void WeatherMap_SerializesAndLoads()
    {
        var map = WeatherMap.Generate(new int2(2, 2), 1);
        var path = Path.GetTempFileName();
        map.Save(path);
        var loaded = WeatherMap.Load(path);
        Assert.Equal(map.Grid.Size, loaded.Grid.Size);
        foreach (var (pos, cell) in map.Grid.Iterate())
        {
            var other = loaded.Grid.Get(pos);
            Assert.Equal(cell.Cloud, other.Cloud, 3);
            Assert.Equal(cell.Rain, other.Rain, 3);
        }
        File.Delete(path);
    }
}
