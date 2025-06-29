using System.IO;
using System.Text.Json;
using VelorenPort.NativeMath;
using VelorenPort.World;

namespace World.Tests;

public class WorldSimReferenceTests
{
    private record RefDto(uint Seed, int[] Size, float[] Alt, float[] Humidity);

    [Fact]
    public void Tick_MatchesReferenceData()
    {
        var path = Path.Combine("Reference", "worldsim_step.json");
        var dto = JsonSerializer.Deserialize<RefDto>(File.ReadAllText(path));
        Assert.NotNull(dto);

        var size = new int2(dto!.Size[0], dto.Size[1]);
        var sim = new WorldSim(dto.Seed, size);
        sim.Tick(1f);

        int idx = 0;
        for (int y = 0; y < size.y; y++)
        for (int x = 0; x < size.x; x++)
        {
            var chunk = sim.Get(new int2(x, y))!;
            Assert.Equal(dto.Alt[idx], chunk.Alt, 3);
            Assert.Equal(dto.Humidity[idx], sim.Humidity.Get(new int2(x, y)), 3);
            idx++;
        }
    }
}
