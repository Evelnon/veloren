using System;
using System.Collections;
using System.Collections.Generic;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Simple list-based path container.
    /// Mirrors the minimal behaviour of common::path::Path.
    /// </summary>
    [Serializable]
    public class Path<T> : IEnumerable<T> {
        private readonly List<T> _nodes;

        public Path() {
            _nodes = new List<T>();
        }

        public Path(IEnumerable<T> nodes) {
            _nodes = new List<T>(nodes);
        }

        public IReadOnlyList<T> Nodes => _nodes;
        public int Length => _nodes.Count;
        public bool IsEmpty => _nodes.Count == 0;
        public T? Start => _nodes.Count > 0 ? _nodes[0] : default;
        public T? End => _nodes.Count > 0 ? _nodes[^1] : default;

        public void Add(T node) => _nodes.Add(node);

        public IEnumerator<T> GetEnumerator() => _nodes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
