using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Extra data generated alongside a chunk. Simplified from
    /// <c>ChunkSupplement</c> in the Rust code.
    /// </summary>
    [Serializable]
    public class ChunkSupplement {
        public List<object> Entities { get; } = new();
        public Dictionary<ChunkResource, int> RtsimMaxResources { get; } = new();
        public List<int3> ResourceBlocks { get; } = new();
        public List<FaunaSpawn> Wildlife { get; } = new();
        public List<int3> SpawnPoints { get; } = new();

        public void AddEntity(object entity) => Entities.Add(entity);
    }
}
