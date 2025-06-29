using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VelorenPort.Server.Ecs {
    /// <summary>
    /// Simple dispatcher that executes registered systems sequentially.
    /// </summary>
    public interface IGameSystem {
        void Update(float dt, Events.EventManager events);
    }

    public class Dispatcher {
        private class Node {
            public IGameSystem System { get; }
            public Type Type { get; }
            public Type[] Deps { get; }

            public Node(IGameSystem system, Type type, Type[] deps) {
                System = system;
                Type = type;
                Deps = deps;
            }
        }

        private readonly List<Node> _nodes = new();
        private readonly int _workers;
        private List<List<Node>>? _schedule;

        public Dispatcher(int workerCount = 0) {
            _workers = workerCount <= 0 ? Environment.ProcessorCount : workerCount;
        }

        public void AddSystem<T>(T system, params Type[] dependsOn) where T : IGameSystem {
            _nodes.Add(new Node(system, typeof(T), dependsOn));
            _schedule = null;
        }

        public void AddSystem(IGameSystem system) => AddSystem(system, Array.Empty<Type>());

        private void BuildSchedule() {
            var map = _nodes.ToDictionary(n => n.Type);
            var indegree = _nodes.ToDictionary(n => n, n => n.Deps.Length);
            var dependents = new Dictionary<Node, List<Node>>();

            foreach (var node in _nodes) {
                foreach (var dep in node.Deps) {
                    if (!map.TryGetValue(dep, out var depNode))
                        throw new InvalidOperationException($"Unknown dependency {dep.Name}");
                    if (!dependents.TryGetValue(depNode, out var list))
                        dependents[depNode] = list = new List<Node>();
                    list.Add(node);
                }
            }

            var queue = new Queue<Node>(_nodes.Where(n => indegree[n] == 0));
            var groups = new List<List<Node>>();

            while (queue.Count > 0) {
                var count = queue.Count;
                var group = new List<Node>(count);
                for (int i = 0; i < count; i++) {
                    var n = queue.Dequeue();
                    group.Add(n);
                }
                groups.Add(group);

                foreach (var n in group) {
                    if (!dependents.TryGetValue(n, out var children)) continue;
                    foreach (var c in children) {
                        indegree[c]--;
                        if (indegree[c] == 0)
                            queue.Enqueue(c);
                    }
                }
            }

            if (groups.Sum(g => g.Count) != _nodes.Count)
                throw new InvalidOperationException("Cycle in dispatcher dependencies");

            _schedule = groups;
        }

        public void Update(float dt, Events.EventManager events) {
            if (_schedule == null) BuildSchedule();
            foreach (var group in _schedule!) {
                var opts = new ParallelOptions { MaxDegreeOfParallelism = _workers };
                Parallel.ForEach(group, opts, n => n.System.Update(dt, events));
            }
        }
    }

    /// <summary>
    /// Utility system wrapper using a delegate.
    /// </summary>
    public class DelegateSystem : IGameSystem {
        private readonly System.Action<float, Events.EventManager> _run;
        public DelegateSystem(System.Action<float, Events.EventManager> run) { _run = run; }
        public void Update(float dt, Events.EventManager events) => _run(dt, events);
    }
}
