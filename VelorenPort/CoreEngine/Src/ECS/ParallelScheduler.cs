using System.Collections.Generic;
using System.Threading.Tasks;

namespace VelorenPort.CoreEngine.ECS;

/// <summary>
/// Executes systems in parallel using <see cref="Task"/>.
/// </summary>
public class ParallelScheduler
{
    private readonly List<EcsSystem> _systems = new();

    public void Add(EcsSystem system) => _systems.Add(system);

    public async Task Run(World world)
    {
        var tasks = new List<Task>();
        foreach (var system in _systems)
            tasks.Add(Task.Run(() => system.Run(world)));
        await Task.WhenAll(tasks);
    }
}
