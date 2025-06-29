using System;
using System.Collections.Generic;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Events {
    /// <summary>
    /// Simple event manager that forwards events to the generic CoreEngine
    /// <see cref="EventBus{T}"/>. It acts as a thin layer so systems can
    /// depend on a single concrete type.
    /// </summary>
    public class EventManager {
        private readonly Dictionary<Type, object> _busses = new();

        private EventBus<T> GetBus<T>() {
            if (!_busses.TryGetValue(typeof(T), out var obj)) {
                obj = new EventBus<T>();
                _busses[typeof(T)] = obj;
            }
            return (EventBus<T>)obj;
        }

        public EventBus<T>.Emitter GetEmitter<T>() => GetBus<T>().GetEmitter();

        public List<T> Drain<T>() => GetBus<T>().RecvAll();

        // Convenience wrappers for legacy callers
        public EventBus<EventType>.Emitter GetEmitter() => GetEmitter<EventType>();
        public EventBus<ChatEvent>.Emitter GetChatEmitter() => GetEmitter<ChatEvent>();

        public EventType[] DrainEvents() => Drain<EventType>().ToArray();
        public ChatEvent[] DrainChatEvents() => Drain<ChatEvent>().ToArray();

#if DEBUG
        /// <summary>
        /// Ensures that all events queued this tick have been consumed by a handler.
        /// Throws <see cref="InvalidOperationException"/> if any remain.
        /// </summary>
        public void DebugCheckAllConsumed()
        {
            foreach (var (ty, obj) in _busses)
            {
                if (obj is IEventBus bus && bus.HasPending)
                    throw new InvalidOperationException($"Event of type {ty.Name} was not consumed");
                if (obj is IEventBus bus2 && bus2.RecvCount > 1)
                    throw new InvalidOperationException($"Event of type {ty.Name} handled multiple times");
            }
        }
#endif
    }
}
