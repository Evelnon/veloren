using System.Threading;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Simple atomic boolean wrapper mimicking Rust's <c>AtomicBool</c>.
    /// </summary>
    public sealed class AtomicBool {
        private int _value;

        public AtomicBool(bool initial = false) {
            _value = initial ? 1 : 0;
        }

        /// <summary>Atomically retrieves the current value.</summary>
        public bool Load() => Interlocked.CompareExchange(ref _value, 0, 0) != 0;

        /// <summary>Atomically sets the value.</summary>
        public void Store(bool value) => Interlocked.Exchange(ref _value, value ? 1 : 0);
    }
}
