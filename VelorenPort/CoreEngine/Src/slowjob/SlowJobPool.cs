using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Simple thread pool for expensive jobs that do not need to finish
    /// in the same frame. Inspired by common::slowjob in Rust but using
    /// idiomatic C# constructs.
    /// </summary>
    [Serializable]
    public class SlowJobPool {
        private class Config {
            public int Limit;
            public int Running;
        }

        private class QueueItem {
            public ulong Id;
            public string Name = string.Empty;
            public int Priority;
            public Action<CancellationToken> Task = null!;
            public CancellationTokenSource? Cts;
        }

        public struct SlowJob {
            public string Name;
            public ulong Id;
        }

        private readonly int _globalLimit;
        private readonly Dictionary<string, Config> _configs = new();
        private readonly Dictionary<string, PriorityQueue<QueueItem, int>> _queue = new();
        private int _running;
        private ulong _nextId;

        public SlowJobPool(int globalLimit) {
            _globalLimit = Math.Max(globalLimit, 1);
        }

        /// <summary>
        /// Configure a named job with a limit relative to the global capacity.
        /// </summary>
        public void Configure(string name, Func<int, int> limitFunc) {
            int limit = Math.Max(1, limitFunc(_globalLimit));
            _configs[name] = new Config { Limit = limit, Running = 0 };
            if (!_queue.ContainsKey(name))
                _queue[name] = new PriorityQueue<QueueItem, int>();
        }

        /// <summary>
        /// Queue a job to be executed when resources are available.
        /// </summary>
        public SlowJob Spawn(string name, Action job, int priority = 0)
            => Spawn(name, _ => job(), priority);

        public SlowJob Spawn(string name, Action<CancellationToken> job, int priority = 0) {
            if (!_configs.ContainsKey(name))
                throw new InvalidOperationException($"Job '{name}' not configured");
            var item = new QueueItem { Id = _nextId++, Name = name, Task = job, Priority = priority };
            _queue[name].Enqueue(item, priority);
            SpawnQueued();
            return new SlowJob { Name = name, Id = item.Id };
        }

        /// <summary>
        /// Attempt to run a job immediately; returns false if limits would be exceeded.
        /// </summary>
        public bool TryRun(string name, Action job, out SlowJob handle, int priority = 0)
            => TryRun(name, _ => job(), out handle, priority);

        public bool TryRun(string name, Action<CancellationToken> job, out SlowJob handle, int priority = 0) {
            if (!CanSpawn(name)) {
                handle = default;
                return false;
            }
            handle = SpawnInternal(name, job, priority);
            return true;
        }

        private bool CanSpawn(string name) {
            if (_running >= _globalLimit) return false;
            if (!_configs.TryGetValue(name, out var cfg)) return false;
            return cfg.Running < cfg.Limit;
        }

        private SlowJob SpawnInternal(string name, Action<CancellationToken> job, int priority) {
            var item = new QueueItem { Id = _nextId++, Name = name, Task = job, Priority = priority };
            StartItem(item);
            return new SlowJob { Name = name, Id = item.Id };
        }

        private void SpawnQueued() {
            foreach (var (name, q) in _queue) {
                while (q.TryPeek(out var item, out _) && CanSpawn(name)) {
                    q.TryDequeue(out item!, out _);
                    StartItem(item);
                }
            }
        }

        private readonly Dictionary<ulong, CancellationTokenSource> _runningTokens = new();

        private void StartItem(QueueItem item) {
            var cfg = _configs[item.Name];
            cfg.Running++;
            _running++;
            _configs[item.Name] = cfg;
            var cts = new CancellationTokenSource();
            item.Cts = cts;
            _runningTokens[item.Id] = cts;
            Task.Run(() => {
                try { item.Task(cts.Token); } finally { Finish(item.Name, item.Id); }
            }, cts.Token);
        }

        private void Finish(string name, ulong id) {
            if (_configs.TryGetValue(name, out var cfg)) {
                cfg.Running = Math.Max(0, cfg.Running - 1);
                _configs[name] = cfg;
            }
            _running = Math.Max(0, _running - 1);
            _runningTokens.Remove(id);
            SpawnQueued();
        }

        public int RunningJobs(string name)
            => _configs.TryGetValue(name, out var cfg) ? cfg.Running : 0;

        public int QueuedJobs(string name)
            => _queue.TryGetValue(name, out var q) ? q.Count : 0;

        /// <summary>
        /// Cancel a job that has not yet started.
        /// </summary>
        public bool Cancel(SlowJob job) {
            if (_queue.TryGetValue(job.Name, out var q)) {
                var items = new List<QueueItem>();
                bool removed = false;
                while (q.TryDequeue(out var item, out _)) {
                    if (!removed && item.Id == job.Id) {
                        removed = true;
                        continue;
                    }
                    items.Add(item);
                }
                foreach (var it in items)
                    q.Enqueue(it, it.Priority);
                return removed;
            }
            return false;
        }

        /// <summary>
        /// Attempt to cancel a running job.
        /// </summary>
        public bool CancelRunning(SlowJob job) {
            if (_runningTokens.Remove(job.Id, out var cts)) {
                cts.Cancel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Spawn an asynchronous job and get a task that completes when done.
        /// </summary>
        public Task SpawnAsync(string name, Func<Task> job, int priority = 0)
        {
            var tcs = new TaskCompletionSource();
            Spawn(name, async _ =>
            {
                await job();
                tcs.SetResult();
            }, priority);
            return tcs.Task;
        }
    }
}
