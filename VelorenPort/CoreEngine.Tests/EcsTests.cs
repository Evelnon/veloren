using VelorenPort.CoreEngine.ECS;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace CoreEngine.Tests;

public class EcsTests
{
    private struct Position : IComponent
    {
        public int Value;
    }

    private class IncrementSystem : EcsSystem
    {
        public override void Run(World world)
        {
            foreach (var (entity, pos) in world.Query<Position>())
            {
                var updated = pos;
                updated.Value++;
                world.Set(entity, updated);
            }
        }
    }

    private class DelayIncrementSystem : EcsSystem
    {
        private readonly int _delayMs;
        public DelayIncrementSystem(int delayMs) => _delayMs = delayMs;

        public override void Run(World world)
        {
            foreach (var (entity, pos) in world.Query<Position>())
            {
                Task.Delay(_delayMs).Wait();
                var p = pos;
                p.Value++;
                world.Set(entity, p);
            }
        }
    }

    [Fact]
    public void CreateEntity_AddAndRetrieveComponent()
    {
        var world = new World();
        var e = world.CreateEntity();
        world.Add(e, new Position { Value = 10 });
        Assert.True(world.Has<Position>(e));
        var p = world.Get<Position>(e);
        Assert.Equal(10, p.Value);
    }

    [Fact]
    public void SystemUpdatesComponents()
    {
        var world = new World();
        var e = world.CreateEntity();
        world.Add(e, new Position { Value = 0 });
        var scheduler = new Scheduler();
        scheduler.Add(new IncrementSystem());
        scheduler.Run(world);
        Assert.Equal(1, world.Get<Position>(e).Value);
    }

    [Fact]
    public void Query_Returns_All_Entries()
    {
        var world = new World();
        var e1 = world.CreateEntity();
        world.Add(e1, new Position { Value = 1 });
        var e2 = world.CreateEntity();
        world.Add(e2, new Position { Value = 2 });

        var results = world.Query<Position>().ToList();
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Item1.Equals(e1) && r.Item2.Value == 1);
        Assert.Contains(results, r => r.Item1.Equals(e2) && r.Item2.Value == 2);
    }

    [Fact]
    public async Task ParallelScheduler_Runs_Systems_Concurrently()
    {
        var world = new World();
        var e = world.CreateEntity();
        world.Add(e, new Position { Value = 0 });

        var scheduler = new ParallelScheduler();
        scheduler.Add(new DelayIncrementSystem(100));
        scheduler.Add(new DelayIncrementSystem(100));

        var sw = Stopwatch.StartNew();
        await scheduler.Run(world);
        sw.Stop();
        Assert.True(sw.ElapsedMilliseconds < 180);
    }
}
