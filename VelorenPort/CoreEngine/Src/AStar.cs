using System;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Generic A* pathfinding algorithm ported with idiomatic C#.
    /// </summary>
    [Serializable]
    public class AStar<S> {
        private int _iter;
        private readonly int _maxIters;
        private float _maxCost;
        private readonly PriorityQueue<(S Node, float CostEst), float> _queue;
        private readonly Dictionary<S, (S CameFrom, float Cost)> _visited;
        private (S Node, float Heuristic)? _closest;

        public AStar(int maxIters, S start, IEqualityComparer<S>? comparer = null) {
            _maxIters = maxIters;
            _maxCost = float.MaxValue;
            _queue = new PriorityQueue<(S, float), float>();
            _queue.Enqueue((start, 0f), 0f);
            _visited = new Dictionary<S, (S, float)>(comparer);
            _visited[start] = (start, 0f);
        }

        public AStar<S> WithMaxCost(float cost) { _maxCost = cost; return this; }

        public PathResult<S> Poll(
            int iters,
            Func<S, float> heuristic,
            Func<S, IEnumerable<(S Node, float Cost)>> neighbors,
            Func<S, bool> satisfied)
        {
            int limit = Math.Min(_maxIters, _iter + iters);
            while (_iter < limit) {
                if (!_queue.TryDequeue(out var entry, out var est))
                    break;
                S node = entry.Node;
                var info = _visited[node];
                float nodeCost = info.Cost;
                S cameFrom = info.CameFrom;

                if (satisfied(node))
                    return new PathResult<S>.Path(Reconstruct(node), nodeCost);
                if (est > _maxCost)
                    return new PathResult<S>.Exhausted(_closest.HasValue ? Reconstruct(_closest.Value.Node) : new Path<S>());

                foreach (var (nb, tCost) in neighbors(node)) {
                    if (_visited.TryGetValue(nb, out var ninfo) && EqualityComparer<S>.Default.Equals(nb, cameFrom))
                        continue;
                    float cost = nodeCost + tCost;
                    if (!_visited.TryGetValue(nb, out ninfo) || cost < ninfo.Cost) {
                        bool prev = _visited.ContainsKey(nb);
                        _visited[nb] = (node, cost);
                        float h = heuristic(nb);
                        float costEst = cost + h;
                        if (!_closest.HasValue || h < _closest.Value.Heuristic)
                            _closest = (nb, h);
                        if (!prev)
                            _queue.Enqueue((nb, costEst), costEst);
                    }
                }
                _iter++;
            }

            if (_queue.Count == 0)
                return new PathResult<S>.None(_closest.HasValue ? Reconstruct(_closest.Value.Node) : new Path<S>());
            if (_iter >= _maxIters)
                return new PathResult<S>.Exhausted(_closest.HasValue ? Reconstruct(_closest.Value.Node) : new Path<S>());
            return new PathResult<S>.Pending();
        }

        private Path<S> Reconstruct(S end) {
            var nodes = new List<S>();
            var current = end;
            while (true) {
                nodes.Add(current);
                var info = _visited[current];
                if (EqualityComparer<S>.Default.Equals(info.CameFrom, current))
                    break;
                current = info.CameFrom;
            }
            nodes.Reverse();
            return new Path<S>(nodes);
        }
    }

    /// <summary>
    /// Result of an A* search iteration.
    /// </summary>
    public abstract record PathResult<T> {
        public sealed record None(Path<T> Closest) : PathResult<T>;
        public sealed record Exhausted(Path<T> Closest) : PathResult<T>;
        public sealed record Path(Path<T> Path, float Cost) : PathResult<T>;
        public sealed record Pending : PathResult<T>;
    }
}
