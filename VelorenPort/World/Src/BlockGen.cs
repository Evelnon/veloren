using System;
using Unity.Mathematics;

namespace VelorenPort.World {
    /// <summary>
    /// Simplified block generator that derives basic terrain blocks from
    /// <see cref="ColumnGen"/> samples. This only approximates the complex logic
    /// of the original Rust <c>BlockGen</c> implementation.
    /// </summary>
    [Serializable]
    public class BlockGen {
        private readonly ColumnGen _columnGen;

        public BlockGen(ColumnGen columnGen) {
            _columnGen = columnGen;
        }

        public ZCache? GetZCache(int2 wpos, WorldIndex index) {
            var sample = _columnGen.Get((wpos, (object)index, (object?)null));
            return sample == null ? null : new ZCache(sample);
        }

        public Block? GetWithZCache(int3 wpos, ZCache? cache) {
            if (cache == null) return null;
            var sample = cache.Sample;
            return wpos.z <= sample.Alt ? new Block(BlockKind.Earth) : (Block?)null;
        }

        public readonly struct ZCache {
            public readonly ColumnSample Sample;
            public ZCache(ColumnSample sample) {
                Sample = sample;
            }
        }
    }
}
