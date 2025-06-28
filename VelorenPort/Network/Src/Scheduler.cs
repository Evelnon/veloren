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
        private int _running;
        private volatile bool _stopped;

        public void Schedule(Func<Task> task) {
            if (_stopped) return;
            _tasks.Enqueue(task);
            if (Interlocked.CompareExchange(ref _running, 1, 0) == 0)
                _ = Task.Run(RunAsync);
        }

        private async Task RunAsync() {
            while (_tasks.TryDequeue(out var t)) {
                try { await t(); } catch { /* ignore */ }
            }
            Interlocked.Exchange(ref _running, 0);
            if (!_stopped && !_tasks.IsEmpty && Interlocked.CompareExchange(ref _running, 1, 0) == 0)
                _ = Task.Run(RunAsync);
        }

        public async Task StopAsync() {
            _stopped = true;
            while (_running != 0)
                await Task.Delay(10);
        }
    }
}
