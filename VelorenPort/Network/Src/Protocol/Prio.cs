using System.Collections.Generic;

namespace VelorenPort.Network.Protocol {
    /// <summary>
    /// Very small priority queue used for message scheduling.
    /// </summary>
    public class Prio {
        private readonly SortedDictionary<byte, Queue<byte[]>> _queues = new();

        public void Enqueue(byte prio, byte[] data) {
            if (!_queues.TryGetValue(prio, out var q)) {
                q = new Queue<byte[]>();
                _queues[prio] = q;
            }
            q.Enqueue(data);
        }

        public bool TryDequeue(out byte[] data) {
            foreach (var kv in _queues) {
                if (kv.Value.Count > 0) {
                    data = kv.Value.Dequeue();
                    return true;
                }
            }
            data = null!;
            return false;
        }
    }
}
