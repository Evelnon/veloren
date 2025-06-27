using System;
using System.Collections.Generic;
using Unity.Mathematics;
using VelorenPort.CoreEngine;
using VelorenPort.World.Sim;

namespace VelorenPort.World {
    /// <summary>
    /// Core world simulation. Most functionality is pending port from Rust.
    /// </summary>
    [Serializable]
    public class WorldSim {
        private readonly Noise _noise;
        private readonly int2 _size;
        private readonly Dictionary<int2, SimChunk> _chunks = new();

        public WorldSim(uint seed, int2 size) {
            _noise = new Noise(seed);
            _size = size;
        }

        public int2 GetSize() => _size;

        public T GetInterpolated<T>(int2 wpos, Func<SimChunk, T> f) where T : struct {
            var chunk = GetWpos(wpos);
            return chunk == null ? default : f(chunk);
        }

        public float GetSurfaceAltApprox(int2 wpos) => GetAltApprox(wpos) ?? 0f;

        public float? GetAltApprox(int2 wpos) => GetWpos(wpos)?.Alt;

        public SimChunk? Get(int2 chunkPos) {
            if (_chunks.TryGetValue(chunkPos, out var chunk)) return chunk;
            return GenerateChunk(chunkPos);
        }

        public SimChunk? GetWpos(int2 wpos) => Get(TerrainChunkSize.WposToCpos(wpos));

        public float? GetGradientApprox(int2 wpos) {
            const int SAMP_RES = 8;
            var altx0 = GetAltApprox(wpos - new int2(SAMP_RES, 0)) ?? 0f;
            var altx1 = GetAltApprox(wpos + new int2(SAMP_RES, 0)) ?? 0f;
            var alty0 = GetAltApprox(wpos - new int2(0, SAMP_RES)) ?? 0f;
            var alty1 = GetAltApprox(wpos + new int2(0, SAMP_RES)) ?? 0f;
            return math.length(new float2(altx1 - altx0, alty1 - alty0)) / SAMP_RES;
        }

        public object? GetNearestPath(int2 wpos) => null;

        private SimChunk GenerateChunk(int2 chunkPos) {
            var worldPos = TerrainChunkSize.CposToWposCenter(chunkPos);
            float baseAlt = _noise.CaveFbm(new float3(worldPos.x * 0.01f, worldPos.y * 0.01f, 0));
            var chunk = new SimChunk {
                Alt = baseAlt * 32f,
                Basement = baseAlt * 31f,
                WaterAlt = 0f,
                Chaos = _noise.Scatter(new float3(worldPos, 0)) * 4f,
                Temp = _noise.Cave(new float3(worldPos, 1)) * 0.5f,
                Humidity = _noise.Scatter(new float3(worldPos, 2)) * 0.5f + 0.5f,
                Rockiness = math.abs(_noise.Cave(new float3(worldPos, 3))),
                TreeDensity = math.saturate(_noise.Scatter(new float3(worldPos,4)) * 0.5f + 0.5f),
                ForestKind = ForestKind.Oak,
                SpawnRate = 1f,
                River = new RiverData(),
                SurfaceVeg = 1f,
                Path = (new Way(), Path.Default),
                CliffHeight = 1f
            };
            _chunks[chunkPos] = chunk;
            return chunk;
        }
    }
}
