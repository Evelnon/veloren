using System;
using System.Threading.Tasks;
using VelorenPort.Network;

namespace Network.Tests;

public class SchedulerTimeoutTests
{
    [Fact]
    public async Task LongRunningTasksIncrementTimeoutCounter()
    {
        var metrics = new Metrics(Pid.NewPid());
        var scheduler = new Scheduler(metrics, maxWorkers: 1, autoScale: false,
            taskTimeout: TimeSpan.FromMilliseconds(30));

        scheduler.Schedule(async () => await Task.Delay(10));
        scheduler.Schedule(async () => await Task.Delay(50));
        scheduler.Schedule(async () => await Task.Delay(60));

        await scheduler.StopAsync(drain: true);

        Assert.Equal(2, scheduler.TimeoutCount);
    }
}
