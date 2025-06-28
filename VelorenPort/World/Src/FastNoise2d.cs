using System;
using Unity.Mathematics;
using VelorenPort.CoreEngine;

namespace VelorenPort.World
{
    /// <summary>
    /// Lightweight 2D noise used for tree clustering and other generation tasks.
    /// Port of <c>FastNoise2d</c> from the Rust code.
    /// </summary>
    [Serializable]
    public struct FastNoise2d
    {
        private readonly RandomField _noise;

        public FastNoise2d(uint seed)
        {
            _noise = new RandomField(seed);
        }

        private float NoiseAt(int2 pos)
        {
            uint val = _noise.Get(new int3(pos.x, pos.y, 0));
            return (val & 4095) * 0.000244140625f; // /4096
        }

        public float Get(double2 pos)
        {
            int2 near = (int2)math.floor(pos);
            float v00 = NoiseAt(near);
            float v10 = NoiseAt(near + new int2(1, 0));
            float v01 = NoiseAt(near + new int2(0, 1));
            float v11 = NoiseAt(near + new int2(1, 1));

            double2 frac = pos - math.floor(pos);
            float2 f = new float2((float)frac.x, (float)frac.y);
            f = f * f * (3f - 2f * f);

            float v0 = v00 + f.x * (v10 - v00);
            float v1 = v01 + f.x * (v11 - v01);
            return (v0 + f.y * (v1 - v0)) * 2f - 1f;
        }
    }
}
