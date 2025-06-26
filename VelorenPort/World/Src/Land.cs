using System;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Wrapper around the world simulation providing helper queries.
    /// Mirrors <c>land.rs</c>.
    /// </summary>
    [Serializable]
    public class Land {
        private readonly WorldSim? _sim; // references sim::WorldSim when available

        private Land(WorldSim? sim) {
            _sim = sim;
        }

        public static Land Empty() => new Land(null);

        public static Land FromSim(WorldSim sim) => new Land(sim);

        /// <summary>Size of the simulated world in chunks.</summary>
        public int2 Size => _sim == null ? new int2(1, 1) : GetSize();

        private int2 GetSize() {
            // Requires WorldSim implementation
            return _sim!.GetSize();
        }

        /// <summary>Retrieve interpolated data from the underlying simulation.</summary>
        public T GetInterpolated<T>(int2 wpos, Func<SimChunk, T> f) where T : struct {
            if (_sim == null) return default;
            return _sim.GetInterpolated(wpos, f) ?? default;
        }

        public float GetSurfaceAltApprox(int2 wpos) {
            if (_sim == null) return 0f;
            return _sim.GetSurfaceAltApprox(wpos);
        }

        public float GetAltApprox(int2 wpos) {
            if (_sim == null) return 0f;
            return _sim.GetAltApprox(wpos) ?? 0f;
        }

        public int2 GetDownhill(int2 wpos) {
            if (_sim == null) return int2.zero;
            var chunk = _sim.GetWpos(wpos);
            return chunk?.Downhill ?? int2.zero;
        }

        public float GetGradientApprox(int2 wpos) {
            if (_sim == null) return 0f;
            return _sim.GetGradientApprox(WposChunkPos(wpos)) ?? 0f;
        }

        public static int2 WposChunkPos(int2 wpos) {
            int2 sz = TerrainChunkSize.RectSize;
            return new int2(DivEuclid(wpos.x, sz.x), DivEuclid(wpos.y, sz.y));
        }

        private static int DivEuclid(int a, int b) {
            int q = a / b;
            int r = a % b;
            if ((r > 0 && b < 0) || (r < 0 && b > 0)) q -= 1;
            return q;
        }

        public SimChunk? GetChunk(int2 chunkPos) {
            if (_sim == null) return null;
            return _sim.Get(chunkPos);
        }

        public SimChunk? GetChunkWpos(int2 wpos) {
            if (_sim == null) return null;
            return _sim.GetWpos(wpos);
        }

        public object? GetNearestPath(int2 wpos) {
            if (_sim == null) return null;
            return _sim.GetNearestPath(wpos);
        }

        public object? ColumnSample(int2 wpos, object index) {
            if (_sim == null) return null;
            var gen = new ColumnGen(_sim);
            return gen.Get((wpos, index, (object?)null));
        }
    }
}
