using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VelorenPort.Network {
    /// <summary>
    /// Very small task scheduler for network operations.
    /// Used to queue work on a single thread to mimic Rust's scheduler.
    /// </summary>
    public class Scheduler {
        private readonly Queue<Func<Task>> _tasks = new();
        private bool _running;

        public void Schedule(Func<Task> task) {
            _tasks.Enqueue(task);
            if (!_running) _ = RunAsync();
        }

        private async Task RunAsync() {
            _running = true;
            while (_tasks.Count > 0) {
                var t = _tasks.Dequeue();
                await t();
            }
            _running = false;
        }
    }
}
