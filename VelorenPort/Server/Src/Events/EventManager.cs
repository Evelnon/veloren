using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Events {
    /// <summary>
    /// Simple event manager that forwards events to the generic CoreEngine
    /// <see cref="EventBus{T}"/>. It acts as a thin layer so systems can
    /// depend on a single concrete type.
    /// </summary>
    public class EventManager {
        private readonly EventBus<EventType> _bus = new();

        public EventBus<EventType>.Emitter GetEmitter() => _bus.GetEmitter();

        public EventType[] DrainEvents() => _bus.RecvAll().ToArray();
    }
}
