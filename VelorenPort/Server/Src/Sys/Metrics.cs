using System;

namespace VelorenPort.Server.Sys {
    /// <summary>
    /// Dummy metrics system that simply prints basic tick information. This is
    /// a far cry from the comprehensive telemetry used by the Rust server but
    /// it allows the rest of the server to call into a metrics API.
    /// </summary>
    public class Metrics {
        private int _ticks;
        private DateTime _start = DateTime.UtcNow;

        /// <summary>Number of ticks recorded since startup.</summary>
        public int Ticks => _ticks;

        public void RecordTick() {
            _ticks++;
            if (_ticks % 60 == 0) {
                var elapsed = DateTime.UtcNow - _start;
                Console.WriteLine($"[Metrics] { _ticks } ticks in { elapsed.TotalSeconds:F1 }s");
            }
        }
    }
}
