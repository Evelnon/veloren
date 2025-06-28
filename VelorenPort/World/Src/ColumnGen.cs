using System;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using VelorenPort.CoreEngine;
using VelorenPort.World.Sim;

namespace VelorenPort.World {
    /// <summary>
    /// Handles sampling of world columns for terrain and object generation.
    /// Partial port of <c>column.rs</c> that queries <see cref="WorldSim"/> and
    /// derives additional properties using local interpolation and noise.
    /// </summary>
    public class ColumnGen {
        private readonly WorldSim _sim;

        public ColumnGen(WorldSim sim) {
            _sim = sim;
        }

        /// <summary>
        /// Retrieve a <see cref="ColumnSample"/> for the given world position.
        /// The logic mirrors the Rust implementation but currently only covers
        /// interpolation of nearby chunk values with a few colour and marble
        /// approximations driven by noise.
        /// </summary>
        public ColumnSample? Get((int2 Wpos, object Index, object? Calendar) input) {
            var wpos = input.Wpos;
            var chunk = _sim.GetWpos(wpos);
            if (chunk == null) return null;

            const int SAMP_RES = 8;
            float altx0 = _sim.GetAltApprox(wpos - new int2(SAMP_RES, 0)) ?? chunk.Alt;
            float altx1 = _sim.GetAltApprox(wpos + new int2(SAMP_RES, 0)) ?? chunk.Alt;
            float alty0 = _sim.GetAltApprox(wpos - new int2(0, SAMP_RES)) ?? chunk.Alt;
            float alty1 = _sim.GetAltApprox(wpos + new int2(0, SAMP_RES)) ?? chunk.Alt;
            float gradient = math.length(new float2(altx1 - altx0, alty1 - alty0)) / SAMP_RES;

            float3 waterVel = chunk.River.Velocity;
            float waterDist = math.abs(chunk.WaterAlt - chunk.Alt);
            float warpFactor = math.saturate(waterDist / 64f);

            float marble = _sim.Noise.CaveFbm(new float3(wpos.x * 0.05f, wpos.y * 0.05f, chunk.Alt * 0.1f));
            float3 baseCol = new float3(0.2f, 0.5f, 0.2f);
            baseCol += marble * 0.1f;
            var surfColor = new Rgb<float>(baseCol.x, baseCol.y, baseCol.z);
            var subColor = new Rgb<float>(baseCol.x * 0.6f, baseCol.y * 0.5f, baseCol.z * 0.5f);

            return new ColumnSample {
                Alt = chunk.Alt,
                RiverlessAlt = chunk.Alt,
                Basement = chunk.Basement,
                Chaos = chunk.Chaos,
                WaterLevel = chunk.WaterAlt,
                WarpFactor = warpFactor,
                SurfaceColor = surfColor,
                SubSurfaceColor = subColor,
                TreeDensity = chunk.TreeDensity,
                ForestKind = chunk.ForestKind,
                Marble = marble,
                MarbleMid = marble * 0.5f,
                MarbleSmall = marble * 0.25f,
                RockDensity = chunk.Rockiness,
                Temp = chunk.Temp,
                Humidity = chunk.Humidity,
                SpawnRate = chunk.SpawnRate,
                StoneCol = new Rgb8(128, 128, 128),
                WaterDist = waterDist,
                Gradient = gradient,
                Path = null,
                SnowCover = false,
                CliffOffset = 0f,
                CliffHeight = chunk.CliffHeight,
                WaterVel = waterVel,
                IceDepth = 0f,
                Chunk = chunk
            };
        }

        public float SampleNoise(int2 wpos) => _sim.Noise.CaveFbm(new float3(wpos.x * 0.1f, wpos.y * 0.1f, 0f));

        /// <summary>
        /// Utility wrapper around <see cref="WorldSim"/> to compute the depth
        /// of water at the given world position.
        /// </summary>
        public float GetWaterDepth(int2 wpos) {
            var chunk = _sim.GetWpos(wpos);
            return chunk == null ? 0f : math.abs(chunk.WaterAlt - chunk.Alt);
        }

        /// <summary>Return an approximated surface normal around <paramref name="wpos"/>.</summary>
        public float3 GetSurfaceNormal(int2 wpos) {
            const int step = 1;
            float altX0 = _sim.GetSurfaceAltApprox(wpos - new int2(step, 0));
            float altX1 = _sim.GetSurfaceAltApprox(wpos + new int2(step, 0));
            float altY0 = _sim.GetSurfaceAltApprox(wpos - new int2(0, step));
            float altY1 = _sim.GetSurfaceAltApprox(wpos + new int2(0, step));
            float3 normal = new float3(altX0 - altX1, altY0 - altY1, 2f);
            return math.normalize(normal);
        }

        public float GetGradientApprox(int2 wpos) => _sim.GetGradientApprox(wpos) ?? 0f;

        public float? GetAltApprox(int2 wpos) => _sim.GetAltApprox(wpos);

        public float GetSurfaceAltApprox(int2 wpos) => _sim.GetSurfaceAltApprox(wpos);
    }
}
