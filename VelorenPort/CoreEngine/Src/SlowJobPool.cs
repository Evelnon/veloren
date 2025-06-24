using System;
using System.Collections.Generic;
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
            public Action Task = null!;
        }

        public struct SlowJob {
            public string Name;
            public ulong Id;
        }

        private readonly int _globalLimit;
        private readonly Dictionary<string, Config> _configs = new();
        private readonly Dictionary<string, Queue<QueueItem>> _queue = new();
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
                _queue[name] = new Queue<QueueItem>();
        }

        /// <summary>
        /// Queue a job to be executed when resources are available.
        /// </summary>
        public SlowJob Spawn(string name, Action job) {
            if (!_configs.ContainsKey(name))
                throw new InvalidOperationException($"Job '{name}' not configured");
            var item = new QueueItem { Id = _nextId++, Name = name, Task = job };
            _queue[name].Enqueue(item);
            SpawnQueued();
            return new SlowJob { Name = name, Id = item.Id };
        }

        /// <summary>
        /// Attempt to run a job immediately; returns false if limits would be exceeded.
        /// </summary>
        public bool TryRun(string name, Action job, out SlowJob handle) {
            if (!CanSpawn(name)) {
                handle = default;
                return false;
            }
            handle = SpawnInternal(name, job);
            return true;
        }

        private bool CanSpawn(string name) {
            if (_running >= _globalLimit) return false;
            if (!_configs.TryGetValue(name, out var cfg)) return false;
            return cfg.Running < cfg.Limit;
        }

        private SlowJob SpawnInternal(string name, Action job) {
            var item = new QueueItem { Id = _nextId++, Name = name, Task = job };
            StartItem(item);
            return new SlowJob { Name = name, Id = item.Id };
        }

        private void SpawnQueued() {
            foreach (var (name, q) in _queue) {
                while (q.Count > 0 && CanSpawn(name)) {
                    StartItem(q.Dequeue());
                }
            }
        }

        private void StartItem(QueueItem item) {
            var cfg = _configs[item.Name];
            cfg.Running++;
            _running++;
            _configs[item.Name] = cfg;
            Task.Run(() => {
                try { item.Task(); } finally { Finish(item.Name); }
            });
        }

        private void Finish(string name) {
            if (_configs.TryGetValue(name, out var cfg)) {
                cfg.Running = Math.Max(0, cfg.Running - 1);
                _configs[name] = cfg;
            }
            _running = Math.Max(0, _running - 1);
            SpawnQueued();
        }

        /// <summary>
        /// Cancel a job that has not yet started.
        /// </summary>
        public bool Cancel(SlowJob job) {
            if (_queue.TryGetValue(job.Name, out var q)) {
                var arr = q.ToArray();
                q.Clear();
                bool removed = false;
                foreach (var item in arr) {
                    if (!removed && item.Id == job.Id) {
                        removed = true;
                        continue;
                    }
                    q.Enqueue(item);
                }
                return removed;
            }
            return false;
        }
    }
}
