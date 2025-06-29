using System;
using System.Collections.Generic;
using System.Threading;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Generic event bus that stores events until they are consumed.
    /// This mirrors the behaviour of the EventBus type in the Rust code.
    /// </summary>
    public class EventBus<T> : IEventBus
    {
        private readonly Queue<T> _queue = new();
        private readonly object _lock = new();
        private byte _recvCount;

        public EventBus() { }

        /// <summary>
        /// Create a disposable emitter that collects events and
        /// pushes them onto the bus when disposed.
        /// </summary>
        public Emitter GetEmitter() => new Emitter(this);

        /// <summary>Emit an event immediately.</summary>
        public void EmitNow(T ev) {
            lock (_lock) {
                _queue.Enqueue(ev);
            }
        }

        /// <summary>
        /// Retrieve all pending events. The returned list has a fixed
        /// size and the bus is cleared afterwards.
        /// </summary>
        public List<T> RecvAll() {
            lock (_lock) {
                var list = new List<T>(_queue);
                _queue.Clear();
                if (_recvCount < byte.MaxValue)
                    _recvCount++;
                return list;
            }
        }

        /// <summary>
        /// Retrieve all pending events assuming exclusive access. This
        /// avoids locking and mirrors <c>recv_all_mut</c> from Rust.
        /// </summary>
        public List<T> RecvAllMut() {
            var list = new List<T>(_queue);
            _queue.Clear();
            if (_recvCount < byte.MaxValue)
                _recvCount++;
            return list;
        }

        /// <summary>
        /// Internal method used by <see cref="Emitter"/> to append events.
        /// </summary>
        internal void AppendRange(IEnumerable<T> events) {
            lock (_lock) {
                foreach (var ev in events)
                    _queue.Enqueue(ev);
            }
        }

        /// <summary>Number of times events have been received (debug only).</summary>
        public byte RecvCount {
            get { lock (_lock) { return _recvCount; } }
        }

        public bool HasPending {
            get { lock (_lock) { return _queue.Count > 0; } }
        }

        /// <summary>
        /// Disposable emitter that batches events and appends them to the bus
        /// when disposed, replicating the RAII drop behaviour of Rust.
        /// </summary>
        public sealed class Emitter : IEmitExt<T>, IDisposable {
            private readonly EventBus<T> _bus;
            private readonly List<T> _events = new();
            private int _disposed;

            internal Emitter(EventBus<T> bus) { _bus = bus; }

            public void Emit(T ev) => _events.Add(ev);

            public void EmitMany(IEnumerable<T> evs) => _events.AddRange(evs);

            public void Append(ref List<T> events) {
                if (events.Count == 0) return;
                _events.AddRange(events);
                events.Clear();
            }

            public void AppendList(List<T> list) {
                if (_events.Count == 0) {
                    _events.AddRange(list);
                } else {
                    _events.AddRange(list);
                }
            }

            public void Dispose() {
                if (Interlocked.Exchange(ref _disposed, 1) == 0 && _events.Count > 0)
                    _bus.AppendRange(_events);
            }
        }
    }

    /// <summary>Interface implemented by types that can emit events.</summary>
    public interface IEmitExt<in T> {
        void Emit(T ev);
        void EmitMany(IEnumerable<T> events);
    }

    /// <summary>Non-generic access to event bus internals for debug checks.</summary>
    public interface IEventBus {
        byte RecvCount { get; }
        bool HasPending { get; }
    }
}
