using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Very small task scheduler for network operations.
    /// Used to queue work on a single thread to mimic Rust's scheduler.
    /// </summary>
    public class Scheduler {
        private readonly ConcurrentQueue<Func<Task>> _tasks = new();
        private int _workers;
        private volatile bool _stopped;
        private readonly Metrics? _metrics;
        private readonly int _maxWorkers;

        private void UpdateWorkersMetric() => _metrics?.SchedulerWorkers(_workers);

        public Scheduler(Metrics? metrics = null, int maxWorkers = 0) {
            _metrics = metrics;
            _maxWorkers = maxWorkers <= 0 ? Environment.ProcessorCount : maxWorkers;
        }

        public void Schedule(Func<Task> task) {
            if (_stopped) return;
            _tasks.Enqueue(task);
            _metrics?.SchedulerQueued(_tasks.Count);
            MaybeStartWorker();
        }

        private void MaybeStartWorker()
        {
            while (!_stopped && _workers < _maxWorkers && !_tasks.IsEmpty)
            {
                if (Interlocked.Increment(ref _workers) <= _maxWorkers)
                {
                    UpdateWorkersMetric();
                    _ = Task.Run(RunAsync);
                }
                else
                {
                    Interlocked.Decrement(ref _workers);
                    break;
                }
            }
        }

        private async Task RunAsync() {
            try
            {
                while (!_stopped && _tasks.TryDequeue(out var t)) {
                    try { await t(); } catch { /* ignore */ }
                    _metrics?.SchedulerTaskExecuted();
                    _metrics?.SchedulerQueued(_tasks.Count);
                }
            }
            finally
            {
                Interlocked.Decrement(ref _workers);
                UpdateWorkersMetric();
                if (!_stopped) MaybeStartWorker();
            }
        }

        public async Task StopAsync() {
            _stopped = true;
            while (_workers > 0)
                await Task.Delay(10);
            _metrics?.SchedulerQueued(_tasks.Count);
            UpdateWorkersMetric();
        }
    }
}
