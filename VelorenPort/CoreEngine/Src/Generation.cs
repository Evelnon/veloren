using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace VelorenPort.CoreEngine {
    /// <summary>
    /// Very small stub for procedural generation utilities.
    /// </summary>
    [Serializable]
    public class Generator {
        private readonly Random _rng;

        public Generator(int seed) {
            _rng = new Random(seed);
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

        public Unity.Mathematics.float2 NextFloat2(float2 min, float2 max) =>
            new Unity.Mathematics.float2(NextFloat(min.x, max.x), NextFloat(min.y, max.y));

        public Unity.Mathematics.float3 NextFloat3(float3 min, float3 max) =>
            new Unity.Mathematics.float3(NextFloat(min.x, max.x), NextFloat(min.y, max.y), NextFloat(min.z, max.z));

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
    }
}
