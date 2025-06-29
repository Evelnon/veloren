using System.Collections.Generic;

namespace VelorenPort.Server.Ecs {
    /// <summary>
    /// Simple dispatcher that executes registered systems sequentially.
    /// </summary>
    public interface IGameSystem {
        void Update(float dt, Events.EventManager events);
    }

    public class Dispatcher {
        private readonly List<IGameSystem> _systems = new();
        public void AddSystem(IGameSystem system) => _systems.Add(system);
        public void Update(float dt, Events.EventManager events) {
            foreach (var s in _systems)
                s.Update(dt, events);
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
