using VelorenPort.CoreEngine;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CoreEngine.Tests;

public class SlowJobPoolPriorityTests
{
    [Fact]
    public async Task HigherPriorityRunsFirst()
    {
        var pool = new SlowJobPool(1);
        pool.Configure("a", _ => 1);
        var order = new List<int>();
        var start1 = new TaskCompletionSource();
        var finish1 = new TaskCompletionSource();
        pool.Spawn("a", ct => { order.Add(1); start1.SetResult(); finish1.Task.Wait(); }, priority:1);
        await start1.Task;
        var t2Done = new TaskCompletionSource();
        var t3Done = new TaskCompletionSource();
        pool.Spawn("a", ct => { order.Add(2); t2Done.SetResult(); }, priority:1);
        pool.Spawn("a", ct => { order.Add(3); t3Done.SetResult(); }, priority:0);
        finish1.SetResult();
        await Task.WhenAll(t2Done.Task, t3Done.Task);
        Assert.Equal(new[] {1,3,2}, order);
    }

    [Fact]
    public async Task CancelRunning_SignalsToken()
    {
        var pool = new SlowJobPool(1);
        pool.Configure("a", _ => 1);
        var tcs = new TaskCompletionSource();
        bool canceled = false;
        var handle = pool.Spawn("a", ct =>
        {
            try
            {
                while (!ct.IsCancellationRequested)
                    Thread.Sleep(5);
                canceled = true;
            }
            finally
            {
                tcs.SetResult();
            }
        });
        await Task.Delay(20);
        var res = pool.CancelRunning(handle);
        await tcs.Task;
        Assert.True(res);
        Assert.True(canceled);
    }
}
