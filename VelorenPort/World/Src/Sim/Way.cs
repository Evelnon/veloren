using System;
using Unity.Mathematics;

namespace VelorenPort.World.Sim {
    /// <summary>
    /// Representation of road connections between chunks.
    /// Mirrors <c>way.rs</c>.
    /// </summary>
    [Serializable]
    public struct Way {
        public int2 Offset;
        public byte Neighbors;

        public bool IsWay => Neighbors != 0;
        public void Clear() => Neighbors = 0;
    }

    [Serializable]
    public struct Path {
        public float Width;
        public static Path Default => new Path { Width = 5f };
    }
}
