using System;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace VelorenPort.World {
    /// <summary>
    /// Noise utilities based on <see cref="Unity.Mathematics.noise"/>.
    /// Provides deterministic patterns that mimic the behaviour of the
    /// original Rust noise generators but with idiomatic C# code.
    /// </summary>
    [Serializable]
    public struct Noise {
        private float3 _caveOffset;
        private float3 _scatterOffset;
        private float3 _caveFbmOffset;

        public Noise(uint seed) {
            var rng = new Unity.Mathematics.Random(seed + 1);
            _caveOffset = rng.NextFloat3();
            _scatterOffset = rng.NextFloat3();
            _caveFbmOffset = rng.NextFloat3();
        }

        /// <summary>Simple 3D noise for caves.</summary>
        public float Cave(float3 pos) => noise.snoise(pos + _caveOffset);

        /// <summary>Simple 3D noise for scatter features.</summary>
        public float Scatter(float3 pos) => noise.snoise(pos + _scatterOffset);

        /// <summary>Fractal Brownian Motion noise for cave variation.</summary>
        public float CaveFbm(float3 pos, int octaves = 5) {
            float value = 0f;
            float amplitude = 0.5f;
            float frequency = 1f;
            float3 p = pos + _caveFbmOffset;
            for (int i = 0; i < octaves; i++) {
                value += noise.snoise(p * frequency) * amplitude;
                frequency *= 2f;
                amplitude *= 0.5f;
            }
            return value;
        }
 NextFloat3() => new float3(NextFloat(), NextFloat(), NextFloat());
    }
}
