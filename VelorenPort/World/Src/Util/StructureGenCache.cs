using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.World.Util
{
    /// <summary>
    /// Caches structure generation results near a queried position.
    /// </summary>
    public class StructureGenCache<T>
    {
        private readonly StructureGen2d _gen;
        private readonly Dictionary<int2, T?> _cache = new();

        public StructureGenCache(StructureGen2d gen)
        {
            _gen = gen;
        }

        /// <summary>
        /// Get cached values for the nine neighbouring samples around
        /// <paramref name="index"/>. Missing entries are generated using
        /// <paramref name="generate"/>.
        /// </summary>
        public IEnumerable<T> Get(int2 index, System.Func<int2, uint, T?> generate)
        {
            var close = _gen.Get(index);
            foreach (var (wpos, seed) in close)
            {
                if (!_cache.ContainsKey(wpos))
                    _cache[wpos] = generate(wpos, seed);
            }
            foreach (var (wpos, _) in close)
            {
                if (_cache[wpos] is T val)
                    yield return val;
            }
        }

        public IEnumerable<T> Generated
        {
            get
            {
                foreach (var val in _cache.Values)
                    if (val is T v)
                        yield return v;
            }
        }
    }
}
