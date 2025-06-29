using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.World;
using VelorenPort.CoreEngine;

namespace VelorenPort.Server.Sys {
    /// <summary>
    /// Simplified terrain synchronization system. Tracks which chunks have been
    /// sent to each client and queues new ones when they enter view distance.
    /// </summary>
    public static class TerrainSync {
        public static void Update(
            WorldIndex index,
            Client client,
            ChunkSerialize serialize,
            List<NpcSpawnerSystem.SpawnPoint> spawnPoints,
            Rtsim.RtSim rtsim) {
            int2 chunkPos = TerrainConstants.WorldToChunk(new int2(
                (int)math.floor(client.Position.Value.x),
                (int)math.floor(client.Position.Value.y)));
            int range = (int)client.Presence.TerrainViewDistance.Current;
            var newLoaded = new HashSet<int2>();
            for (int dx = -range; dx <= range; dx++)
            for (int dy = -range; dy <= range; dy++) {
                var key = new int2(chunkPos.x + dx, chunkPos.y + dy);
                newLoaded.Add(key);
                if (!client.LoadedChunks.Contains(key)) {
                    var (chunk, sup) = index.Map.GetOrGenerateWithSupplement(key, index.Noise);
                    serialize.Queue(key);

                    foreach (var pos in sup.SpawnPoints)
                        spawnPoints.Add(new NpcSpawnerSystem.SpawnPoint {
                            Position = (VelorenPort.NativeMath.float3)pos,
                            Interval = 5f,
                            Timer = 0f,
                            MaxNpcs = 1
                        });

                    rtsim.ResourceCounter += sup.ResourceDeposits.Count;
                }
            }
            client.LoadedChunks.RemoveWhere(k => !newLoaded.Contains(k));
            foreach (var k in newLoaded)
                client.LoadedChunks.Add(k);
        }
    }
}
