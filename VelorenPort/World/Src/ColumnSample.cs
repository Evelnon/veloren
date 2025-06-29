using System;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;
using VelorenPort.World.Sim;

namespace VelorenPort.World {
    /// <summary>
    /// Data sampled for a single world column. Mirrors the ColumnSample struct
    /// from the Rust implementation in <c>column.rs</c>.
    /// </summary>
    [Serializable]
    public class ColumnSample {
        public float Alt { get; set; }
        public float RiverlessAlt { get; set; }
        public float Basement { get; set; }
        public float Chaos { get; set; }
        public float WaterLevel { get; set; }
        public float WarpFactor { get; set; }
        public Rgb<float> SurfaceColor { get; set; }
        public Rgb<float> SubSurfaceColor { get; set; }
        public float TreeDensity { get; set; }
        public ForestKind ForestKind { get; set; }
        public float Marble { get; set; }
        public float MarbleMid { get; set; }
        public float MarbleSmall { get; set; }
        public float RockDensity { get; set; }
        public float Temp { get; set; }
        public float Humidity { get; set; }
        public float SpawnRate { get; set; }
        public Rgb8 StoneCol { get; set; }
        public float? WaterDist { get; set; }
        public float? Gradient { get; set; }
        public (float dist, float2 pos, Path path, float2 dir)? Path { get; set; }
        public bool SnowCover { get; set; }
        public float CliffOffset { get; set; }
        public float CliffHeight { get; set; }
        public float3 WaterVel { get; set; }
        public float IceDepth { get; set; }

        public SimChunk Chunk { get; set; } = null!;

        /// <summary>
        /// Extract a lightweight information struct for easier passing around.
        /// </summary>
        public ColInfo GetInfo() => new ColInfo {
            Alt = Alt,
            RiverlessAlt = RiverlessAlt,
            Basement = Basement,
            CliffOffset = CliffOffset,
            CliffHeight = CliffHeight
        };
    }

    /// <summary>
    /// Portable subset of <see cref="ColumnSample"/> used by various systems.
    /// </summary>
    [Serializable]
    public struct ColInfo {
        public float Alt;
        public float RiverlessAlt;
        public float Basement;
        public float CliffOffset;
        public float CliffHeight;
    }
}
