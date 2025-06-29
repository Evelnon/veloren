using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using Rng = System.Random;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Very small stub for procedural generation utilities.
    /// </summary>
    [Serializable]
    public class Generator {
        private readonly Rng _rng;

        public Generator(int seed) {
            _rng = new Rng(seed);
        }

        /// <summary>Create a random identifier. Placeholder for complex logic.</summary>
        public Uid NextUid() => new Uid((uint)_rng.Next());

        /// <summary>Pick an element from the list uniformly.</summary>
        public T Choose<T>(IReadOnlyList<T> list) => list[_rng.Next(list.Count)];

        /// <summary>Generate a float in the given range.</summary>
        public float NextFloat(float min, float max) =>
            (float)_rng.NextDouble() * (max - min) + min;

        /// <summary>Generate a double in the range [0,1).</summary>
        public double NextDouble() => _rng.NextDouble();

        public VelorenPort.NativeMath.float2 NextFloat2(float2 min, float2 max) =>
            new VelorenPort.NativeMath.float2(NextFloat(min.x, max.x), NextFloat(min.y, max.y));

        public VelorenPort.NativeMath.float3 NextFloat3(float3 min, float3 max) =>
            new VelorenPort.NativeMath.float3(NextFloat(min.x, max.x), NextFloat(min.y, max.y), NextFloat(min.z, max.z));

        /// <summary>Generate a random integer in the inclusive range.</summary>
        public int NextInt(int min, int max) => _rng.Next(min, max + 1);

        /// <summary>Return true with probability <paramref name="p"/>.</summary>
        public bool Chance(double p) => _rng.NextDouble() < p;

        /// <summary>Choose an element weighted by the provided scores.</summary>
        public T Weighted<T>(IReadOnlyList<T> items, IReadOnlyList<float> weights)
        {
            if (items.Count != weights.Count || items.Count == 0)
                throw new ArgumentException("Mismatched item/weight lengths");
            float total = 0f;
            foreach (var w in weights) total += w;
            float pick = (float)_rng.NextDouble() * total;
            for (int i = 0; i < items.Count; i++)
            {
                if (pick < weights[i]) return items[i];
                pick -= weights[i];
            }
            return items[^1];
        }

        /// <summary>
        /// Sample 2D simplex noise with frequency control. Useful for basic
        /// terrain or pattern generation independent of Unity libraries.
        /// </summary>
        public float SampleNoise2D(float2 pos, float frequency = 1f) {
            return noise.snoise(new float3(pos.x * frequency, pos.y * frequency, 0f));
        }

        /// <summary>
        /// Sample 3D simplex noise.
        /// </summary>
        public float SampleNoise3D(float3 pos, float frequency = 1f) {
            return noise.snoise(pos * frequency);
        }

        /// <summary>
        /// Convenience method to pick a color uniformly.
        /// </summary>
        public Rgb8 NextColor() => new Rgb8((byte)_rng.Next(256), (byte)_rng.Next(256), (byte)_rng.Next(256));
    }
}
