using System;
using System.Collections.Generic;
using Unity.Mathematics;

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
}
