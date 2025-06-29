using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.World
{
    /// <summary>
    /// Very lightweight region model used during world simulation.
    /// Stores a rolling history of recent events similarly to the Rust
    /// implementation but without persistence.
    /// </summary>
    [Serializable]
    public class Region
    {
        private readonly Queue<string> _events = new();
        private const int MaxHistory = 20;

        public int2 ChunkPos { get; }
        public IEnumerable<string> Events => _events;

        public Region(int2 chunkPos)
        {
            ChunkPos = chunkPos;
        }

        /// <summary>Add a new event to the region history.</summary>
        public void AddEvent(string desc)
        {
            if (_events.Count >= MaxHistory)
                _events.Dequeue();
            _events.Enqueue(desc);
        }

        /// <summary>Update internal state each tick.</summary>
        public void Tick()
        {
            // Placeholder for future simulation logic.
        }
    }
}
