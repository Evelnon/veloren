using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.World.Sim {
    /// <summary>
    /// Extremely simplified erosion routine. It computes a downhill
    /// vector for every loaded chunk and slightly lowers the altitude
    /// towards that neighbour. This is only a placeholder until the
    /// full erosion model is ported from Rust.
    /// </summary>
    public static class Erosion {
        public static void Apply(WorldSim sim) {
            var edits = new List<(int2 pos, float alt, int2? downhill)>();
            foreach (var (pos, chunk) in sim.Chunks) {
                float bestAlt = chunk.Alt;
                int2? downhill = null;
                foreach (var off in WorldUtil.NEIGHBORS) {
                    var n = sim.Get(pos + off);
                    if (n == null) continue;
                    if (n.Alt < bestAlt) {
                        bestAlt = n.Alt;
                        downhill = TerrainChunkSize.CposToWpos(pos + off);
                    }
                }
                edits.Add((pos, math.lerp(chunk.Alt, bestAlt, 0.05f), downhill));
            }
            foreach (var (pos, alt, downhill) in edits) {
                var chunk = sim.Get(pos);
                if (chunk == null) continue;
                chunk.Alt = alt;
                chunk.Downhill = downhill;
            }
        }

        /// <summary>
        /// Raise local minima so water can flow between chunks. This performs a
        /// very naive sink filling pass over the currently loaded chunks.
        /// </summary>
        public static void FillSinks(WorldSim sim, float epsilon = 0.01f)
        {
            bool changed;
            do
            {
                changed = false;
                foreach (var (pos, chunk) in sim.Chunks)
                {
                    float target = chunk.Alt;
                    foreach (var off in WorldUtil.NEIGHBORS)
                    {
                        var n = sim.Get(pos + off);
                        if (n != null)
                            target = math.min(target, n.Alt + epsilon);
                    }
                    if (target > chunk.Alt + epsilon)
                    {
                        chunk.Alt = target;
                        changed = true;
                    }
                }
            } while (changed);
        }
    }
}
