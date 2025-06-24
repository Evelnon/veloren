using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Simplified game clock inspired by the Rust implementation.
    /// Keeps a target delta time and provides statistics over recent ticks.
    /// </summary>
    public class Clock {
        private TimeSpan _targetDt;
        private DateTime _lastTime;
        private TimeSpan _lastDt;
        private readonly Queue<double> _lastDts = new();
        private readonly Queue<double> _lastBusyDts = new();
        private ClockStats _stats = new ClockStats(Array.Empty<double>(), new Queue<double>());
        private TimeSpan _totalTickTime = TimeSpan.Zero;

        public const int History = 100;
        private const int Compared = 5;

        public Clock(TimeSpan targetDt) {
            _targetDt = targetDt;
            _lastTime = DateTime.UtcNow;
            _lastDt = targetDt;
        }

        public TimeSpan Dt => _lastDt;
        public ClockStats Stats => _stats;
        public TimeSpan TotalTickTime => _totalTickTime;

        public void SetTargetDt(TimeSpan dt) => _targetDt = dt;

        public TimeSpan GetStableDt() {
            if (_lastDts.Count >= Compared) {
                var arr = _lastDts.Reverse().Take(Compared).ToArray();
                Array.Sort(arr);
                var stable = TimeSpan.FromSeconds(arr[Compared / 2]);
                if (_lastDt > stable + stable) {
                    return stable;
                }
            }
            return _lastDt;
        }

        public void Tick() {
            var now = DateTime.UtcNow;
            var busy = now - _lastTime;
            if (_targetDt > busy) {
                Thread.Sleep(_targetDt - busy);
            }
            var afterSleep = DateTime.UtcNow;
            _lastDt = afterSleep - _lastTime;
            _lastTime = afterSleep;
            _totalTickTime += _lastDt;

            if (_lastDts.Count >= History) _lastDts.Dequeue();
            if (_lastBusyDts.Count >= History) _lastBusyDts.Dequeue();
            _lastDts.Enqueue(_lastDt.TotalSeconds);
            _lastBusyDts.Enqueue(busy.TotalSeconds);

            var sorted = _lastDts.OrderBy(x => x).ToArray();
            _stats = new ClockStats(sorted, new Queue<double>(_lastBusyDts));
        }
    }

    public class ClockStats {
        public TimeSpan AverageBusyDt { get; }
        public double AverageTps { get; }
        public double MedianTps { get; }
        public double Percentile90Tps { get; }
        public double Percentile95Tps { get; }
        public double Percentile99Tps { get; }

        public ClockStats(IEnumerable<double> sortedDts, Queue<double> busyList) {
            var dts = sortedDts.ToArray();
            var busy = busyList.ToArray();
            double avgDt = dts.Length > 0 ? dts.Average() : 0.0;
            double avgBusy = busy.Length > 0 ? busy.Average() : 0.0;
            AverageBusyDt = TimeSpan.FromSeconds(avgBusy);
            AverageTps = avgDt > 0 ? 1.0 / avgDt : 0.0;
            MedianTps = dts.Length > 0 ? 1.0 / dts[dts.Length / 2] : 0.0;
            Percentile90Tps = dts.Length >= Clock.History ? 1.0 / dts[(int)(dts.Length * 0.10)] : MedianTps;
            Percentile95Tps = dts.Length >= Clock.History ? 1.0 / dts[(int)(dts.Length * 0.05)] : MedianTps;
            Percentile99Tps = dts.Length >= Clock.History ? 1.0 / dts[(int)(dts.Length * 0.01)] : MedianTps;
        }
    }
}
