using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;
using VelorenPort.Server;
using VelorenPort.World;

namespace VelorenPort.Simulation {
    /// <summary>
    /// Central dispatcher for rtsim events. Provides minimal event routing
    /// capabilities mirroring the original Rust implementation.
    /// </summary>
    [Serializable]
    public class RtState {
        private readonly Dictionary<Type, object> _resources = new();
        private readonly Dictionary<Type, IRule> _rules = new();
        private readonly Dictionary<Type, List<IEventHandler>> _handlers = new();

        public void AddResource<T>(T resource) where T : class =>
            _resources[typeof(T)] = resource;

        public T GetResource<T>() where T : class =>
            (T)_resources[typeof(T)];

        public void AddRule<R>(R rule) where R : IRule
        {
            _rules[typeof(R)] = rule;
            rule.Start(this);
        }

        public void StartRule<R>() where R : IRule, new()
        {
            var rule = new R();
            AddRule(rule);
        }

        public R GetRule<R>() where R : IRule
        {
            if (_rules.TryGetValue(typeof(R), out var rule))
                return (R)rule;
            throw new InvalidOperationException($"Rule '{typeof(R).Name}' does not exist");
        }

        public void Bind<R, E, D>(Action<EventCtx<R, E, D>> handler)
            where R : IRule
            where E : IEvent<D>
        {
            if (!_handlers.TryGetValue(typeof(E), out var list)) {
                list = new List<IEventHandler>();
                _handlers[typeof(E)] = list;
            }
            list.Add(new EventHandler<R, E, D>(handler));
        }

        public void Emit<E, D>(E evt, D data, World world, TestWorld.IndexRef index)
            where E : IEvent<D>
        {
            if (_handlers.TryGetValue(typeof(E), out var list)) {
                foreach (var h in list)
                    h.Invoke(this, world, index, evt!, data!);
            }
        }

        public void Tick(
            NpcSystemData systemData,
            World world,
            TestWorld.IndexRef index,
            TimeOfDay timeOfDay,
            Time time,
            float dt)
        {
            var data = GetResource<Data>();
            data.TimeOfDay = timeOfDay;
            data.Tick++;
            var evt = new OnTick(timeOfDay, time, data.Tick, dt);
            Emit(evt, systemData, world, index);
        }

        private interface IEventHandler {
            void Invoke(RtState state, World world, TestWorld.IndexRef index, object evt, object data);
        }

        private sealed class EventHandler<R, E, D> : IEventHandler
            where R : IRule
            where E : IEvent<D>
        {
            private readonly Action<EventCtx<R, E, D>> _handler;
            public EventHandler(Action<EventCtx<R, E, D>> handler) => _handler = handler;

            public void Invoke(RtState state, World world, TestWorld.IndexRef index, object evt, object data)
            {
                var rule = state.GetRule<R>();
                var ctx = new EventCtx<R, E, D>(state, rule, (E)evt, world, index, (D)data);
                _handler(ctx);
            }
        }
    }
}
