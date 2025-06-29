using VelorenPort.CoreEngine;

namespace CoreEngine.Tests;

public class SlowJobPoolAdvancedTests
{
    [Fact]
    public async Task SpawnAsync_CompletesTask()
    {
        var pool = new SlowJobPool(1);
        pool.Configure("test", _ => 1);
        bool run = false;
        await pool.SpawnAsync("test", async () => { run = true; await Task.Delay(10); });
        Assert.True(run);
    }

    [Fact]
    public void RunningAndQueued_ReportCounts()
    {
        var pool = new SlowJobPool(1);
        pool.Configure("a", _ => 1);
        pool.Spawn("a", () => Task.Delay(50).Wait());
        pool.Spawn("a", () => {});
        Assert.Equal(1, pool.RunningJobs("a"));
        Assert.Equal(1, pool.QueuedJobs("a"));
    }
}
