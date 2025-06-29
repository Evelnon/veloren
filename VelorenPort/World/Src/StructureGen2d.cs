using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;
using VelorenPort.CoreEngine;

namespace VelorenPort.World
{
    /// <summary>
    /// Generates deterministic positions and seeds for procedural structures.
    /// Based on the Rust <c>StructureGen2d</c> implementation.
    /// </summary>
    [Serializable]
    public struct StructureGen2d
    {
        private readonly uint _freq;
        private readonly uint _spread;
        private readonly RandomField _xField;
        private readonly RandomField _yField;
        private readonly RandomField _seedField;

        public StructureGen2d(uint seed, uint freq, uint spread)
        {
            _freq = freq;
            _spread = spread;
            _xField = new RandomField(seed + 0);
            _yField = new RandomField(seed + 1);
            _seedField = new RandomField(seed + 2);
        }

        private static int2 SampleToIndexInternal(int freq, int2 pos) => pos / freq;

        public int2 SampleToIndex(int2 pos) => SampleToIndexInternal((int)_freq, pos);

        private static int FreqOffset(int freq) => freq / 2;

        private static uint SpreadMul(uint spread) => spread * 2u;

        private static (int2 pos, uint seed) IndexToSampleInternal(
            int freq, int freqOffset, int spread, uint spreadMul,
            RandomField xField, RandomField yField, RandomField seedField, int2 index)
        {
            int2 center = index * freq + freqOffset;
            int3 p = new int3(center.x, center.y, 0);
            int2 offset = spreadMul > 0
                ? new int2((int)(xField.Get(p) % spreadMul) - spread,
                           (int)(yField.Get(p) % spreadMul) - spread)
                : int2.zero;
            return (center + offset, seedField.Get(p));
        }

        public IEnumerable<(int2 pos, uint seed)> Iter(int2 min, int2 max)
        {
            uint spreadMul = SpreadMul(_spread);
            int spread = (int)_spread;
            int freq = (int)_freq;
            int freqOffset = FreqOffset(freq);

            int2 minIndex = SampleToIndexInternal(freq, min) - 1;
            int2 maxIndex = SampleToIndexInternal(freq, max) + 1;
            uint xlen = (uint)(maxIndex.x - minIndex.x);
            uint ylen = (uint)(maxIndex.y - minIndex.y);
            ulong len = (ulong)xlen * ylen;
            for (ulong xy = 0; xy < len; xy++)
            {
                int2 index = minIndex + new int2((int)(xy % xlen), (int)(xy / xlen));
                yield return IndexToSampleInternal(freq, freqOffset, spread, spreadMul,
                    _xField, _yField, _seedField, index);
            }
        }

        public (int2 pos, uint seed)[] Get(int2 samplePos)
        {
            uint spreadMul = SpreadMul(_spread);
            int spread = (int)_spread;
            int freq = (int)_freq;
            int freqOffset = FreqOffset(freq);
            int2 closest = SampleToIndexInternal(freq, samplePos);
            var result = new (int2, uint)[9];
            for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                int2 index = closest + new int2(i, j) - 1;
                result[i * 3 + j] = IndexToSampleInternal(freq, freqOffset, spread,
                    spreadMul, _xField, _yField, _seedField, index);
            }
            return result;
        }
    }
}
