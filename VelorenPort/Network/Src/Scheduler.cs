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
        private int _maxWorkers;
        private int _configuredWorkers;
        private bool _autoScale;
        private Timer? _scaleTimer;
        private long _executed;
        private DateTime _lastLoadUpdate;
        private TimeSpan? _taskTimeout;
        private Action<string>? _timeoutLogger;

        private void UpdateWorkersMetric() => _metrics?.SchedulerWorkers(_workers);

        public Scheduler(
            Metrics? metrics = null,
            int maxWorkers = 0,
            bool autoScale = false,
            TimeSpan? taskTimeout = null,
            Action<string>? timeoutLogger = null)
        {
            _metrics = metrics;
            _configuredWorkers = maxWorkers <= 0 ? Environment.ProcessorCount : maxWorkers;
            _maxWorkers = _configuredWorkers;
            _autoScale = autoScale;
            _taskTimeout = taskTimeout;
            _timeoutLogger = timeoutLogger;
            _lastLoadUpdate = DateTime.UtcNow;
            if (_autoScale)
                _scaleTimer = new Timer(_ => AdjustWorkers(), null, 1000, 1000);
        }

        public void ConfigureTimeout(TimeSpan? timeout, Action<string>? logger = null)
        {
            _taskTimeout = timeout;
            _timeoutLogger = logger;
        }

        public void SetMaxWorkers(int workers)
        {
            if (workers <= 0) workers = Environment.ProcessorCount;
            _configuredWorkers = workers;
            if (!_autoScale)
            {
                _maxWorkers = _configuredWorkers;
                MaybeStartWorker();
            }
        }

        public void Schedule(Func<Task> task) {
            if (_stopped) return;
            _tasks.Enqueue(task);
            _metrics?.SchedulerQueued(_tasks.Count);
            MaybeStartWorker();
        }

        public void EnableAutoScale(bool enable)
        {
            if (_autoScale == enable) return;
            _autoScale = enable;
            if (enable)
            {
                _scaleTimer = new Timer(_ => AdjustWorkers(), null, 1000, 1000);
            }
            else
            {
                _scaleTimer?.Dispose();
                _scaleTimer = null;
                _maxWorkers = _configuredWorkers;
                MaybeStartWorker();
            }
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

        private void AdjustWorkers()
        {
            if (_stopped) return;
            int queue = _tasks.Count;
            int target = Math.Clamp(queue / 10 + 1, 1, _configuredWorkers);
            _maxWorkers = target;
            MaybeStartWorker();
        }

        private async Task RunAsync() {
            try
            {
                while (true)
                {
                    while (!_stopped && _tasks.TryDequeue(out var t))
                    {
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        try { await t(); } catch { /* ignore */ }
                        sw.Stop();
                        _metrics?.SchedulerTaskExecuted();
                        _metrics?.SchedulerTaskDuration(sw.Elapsed.TotalSeconds);
                        if (_taskTimeout.HasValue && sw.Elapsed > _taskTimeout.Value)
                        {
                            var name = $"{t.Method.DeclaringType?.Name}.{t.Method.Name}";
                            _timeoutLogger?.Invoke($"Scheduler task '{name}' exceeded {_taskTimeout.Value.TotalMilliseconds}ms (took {sw.Elapsed.TotalMilliseconds:F0}ms)");
                        }
                        Interlocked.Increment(ref _executed);
                        _metrics?.SchedulerQueued(_tasks.Count);
                        ReportLoad();
                    }

                    if (_stopped || _workers > _maxWorkers || _tasks.IsEmpty)
                        break;
                }
            }
            finally
            {
                Interlocked.Decrement(ref _workers);
                UpdateWorkersMetric();
                if (!_stopped) MaybeStartWorker();
            }
        }

        private void ReportLoad()
        {
            var now = DateTime.UtcNow;
            var delta = (now - _lastLoadUpdate).TotalSeconds;
            if (delta >= 1)
            {
                var exec = Interlocked.Exchange(ref _executed, 0);
                _metrics?.SchedulerLoad(exec / delta);
                _lastLoadUpdate = now;
            }
        }

        public async Task StopAsync(bool drain = false) {
            _stopped = true;
            _scaleTimer?.Dispose();
            if (drain)
            {
                while (!_tasks.IsEmpty)
                    await Task.Delay(10);
            }
            while (_workers > 0)
                await Task.Delay(10);
            _metrics?.SchedulerQueued(_tasks.Count);
            UpdateWorkersMetric();
            ReportLoad();
        }
    }
}
