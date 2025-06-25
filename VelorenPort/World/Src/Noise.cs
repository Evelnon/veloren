using System;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Simple noise generator used during the early stages of the port.
    /// It provides deterministic random values based on a seed.
    /// </summary>
    [Serializable]
    public struct Noise {
        private Random _rng;

        public Noise(uint seed) {
            _rng = new Random(unchecked((int)seed));
        }

        /// <summary>
        /// Return a random value in [0,1).
        /// </summary>
        public float NextFloat() => (float)_rng.NextDouble();

        public float2 NextFloat2() => new float2(NextFloat(), NextFloat());
        public float3 NextFloat3() => new float3(NextFloat(), NextFloat(), NextFloat());
    }
}
