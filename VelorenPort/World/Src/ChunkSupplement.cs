using System;
using System.Collections.Generic;

namespace VelorenPort.World {
    /// <summary>
    /// Extra data generated alongside a chunk. Simplified from
    /// <c>ChunkSupplement</c> in the Rust code.
    /// </summary>
    [Serializable]
    public class ChunkSupplement {
        public List<object> Entities { get; } = new();
        public Dictionary<ChunkResource, int> RtsimMaxResources { get; } = new();

        public void AddEntity(object entity) => Entities.Add(entity);
    }
}
