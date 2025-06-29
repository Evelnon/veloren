using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Sim {
    /// <summary>
    /// Type of water body for a chunk.
    /// Mirrors <c>RiverKind</c> from erosion.rs.
    /// </summary>
    public enum RiverKind {
        Ocean,
        Lake,
        River
    }

    /// <summary>
    /// Data describing a river segment.
    /// This is a simplified representation while full erosion logic is ported.
    /// </summary>
    [Serializable]
    public class RiverData {
        public float3 Velocity { get; set; }
        public float2 SplineDerivative { get; set; }
        public RiverKind? Kind { get; set; }
        public List<uint> NeighborRivers { get; } = new();

        public bool IsOcean => Kind == RiverKind.Ocean;
        public bool IsRiver => Kind == RiverKind.River;
        public bool IsLake => Kind == RiverKind.Lake;
        public bool NearRiver => IsRiver || NeighborRivers.Count > 0;
        public bool NearWater => NearRiver || IsLake || IsOcean;
    }

    /// <summary>
    /// Simple river path carving. Marks any chunk with flux above a threshold
    /// as part of a river and sets its spline derivative based on the downhill
    /// direction.
    /// </summary>
    public static class River {
        public static void CarvePaths(WorldSim sim, float fluxThreshold = 4f) {
            var size = sim.GetSize();
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++) {
                var pos = new int2(x, y);
                var chunk = sim.Get(pos);
                if (chunk == null) continue;

                chunk.River.Kind = null;
                chunk.River.SplineDerivative = float2.zero;

                if (chunk.Flux > fluxThreshold && chunk.Downhill.HasValue) {
                    int2 dpos = TerrainChunkSize.WposToCpos(chunk.Downhill.Value);
                    float2 dir = (float2)(dpos - pos);
                    if (math.lengthsq(dir) > 0f)
                        dir = math.normalize(dir);
                    chunk.River.Kind = RiverKind.River;
                    chunk.River.SplineDerivative = dir;
                    chunk.River.Velocity = new float3(dir.x, dir.y, 0f) * chunk.Flux;
                }
            }
        }

        /// <summary>
        /// Form simple deltas by depositing sediment at river mouths.
        /// Any river chunk without a downhill neighbor deposits its stored
        /// sediment onto the terrain height.
        /// </summary>
        public static void FormDeltas(WorldSim sim, float sedimentThreshold = 0f)
        {
            var size = sim.GetSize();
            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
            {
                var chunk = sim.Get(new int2(x, y));
                if (chunk == null) continue;
                if (chunk.River.IsRiver && !chunk.Downhill.HasValue && chunk.Sediment > sedimentThreshold)
                {
                    chunk.Alt += chunk.Sediment;
                    chunk.Sediment = 0f;
                }
            }
        }
    }
}
