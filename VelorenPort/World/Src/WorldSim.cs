using System;
using Unity.Mathematics;
using VelorenPort.CoreEngine;
using VelorenPort.World.Sim;

namespace VelorenPort.World {
    /// <summary>
    /// Core world simulation. Most functionality is pending port from Rust.
    /// </summary>
    [Serializable]
    public class WorldSim {
        public int2 GetSize() => throw new NotImplementedException();
        public T GetInterpolated<T>(int2 wpos, Func<SimChunk, T> f) where T : struct => throw new NotImplementedException();
        public float GetSurfaceAltApprox(int2 wpos) => throw new NotImplementedException();
        public float? GetAltApprox(int2 wpos) => throw new NotImplementedException();
        public SimChunk? Get(int2 chunkPos) => throw new NotImplementedException();
        public SimChunk? GetWpos(int2 wpos) => throw new NotImplementedException();
        public float? GetGradientApprox(int2 chunkPos) => throw new NotImplementedException();
        public object? GetNearestPath(int2 wpos) => throw new NotImplementedException();
    }
}
