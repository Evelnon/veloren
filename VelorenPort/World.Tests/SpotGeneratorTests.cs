using System;
using System.IO;
using VelorenPort.World;
using Unity.Mathematics;

namespace World.Tests;

public class SpotGeneratorTests
{
    [Fact]
    public void Generate_AddsSpotsToChunks()
    {
        var sim = new WorldSim(0, new int2(8, 8));
        SpotGenerator.Generate(sim, 0.1f, new System.Random(0));
        bool found = false;
        foreach (var (_, chunk) in sim.Chunks)
            if (chunk.Spot != null)
            {
                found = true;
                break;
            }
        Assert.True(found);
    }

    [Fact]
    public void Generate_RespectsManifest()
    {
        var sim = new WorldSim(0, new int2(8, 8));
        string dir = Path.Combine(AppContext.BaseDirectory,
            "../../../../../Assets/Spots");
        var manifest = SpotManifest.LoadFromDir(dir);
        SpotGenerator.Generate(sim, 0.1f, new System.Random(0), manifest);
        foreach (var (_, chunk) in sim.Chunks)
            Assert.NotEqual(Spot.DwarvenGrave, chunk.Spot);
    }
}
