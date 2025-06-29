using VelorenPort.CoreEngine.ECS;

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
            foreach (var entity in world.EntitiesWith<Position>())
            {
                var pos = world.Get<Position>(entity);
                pos.Value++;
                world.Set(entity, pos);
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
}
