using System;
using System.Collections.Generic;
using VelorenPort.NativeMath;

namespace VelorenPort.CoreEngine.figure
{
    /// <summary>
    /// Helper to combine multiple <see cref="Dyna{V,M}"/> volumes into a single
    /// larger volume. This mirrors the `DynaUnionizer` struct from the Rust
    /// project but is implemented in a simplified form.
    /// </summary>
    /// <typeparam name="V">Voxel type</typeparam>
    public class DynaUnionizer<V>
    {
        private readonly List<(Dyna<V, object> vol, int3 offset)> _vols = new();
        private readonly Func<V, bool> _isFilled;
        private readonly Func<V> _defaultNonFilled;

        public DynaUnionizer(Func<V, bool> isFilled, Func<V> defaultNonFilled)
        {
            _isFilled = isFilled;
            _defaultNonFilled = defaultNonFilled;
        }

        /// <summary>Add a new volume at the given offset.</summary>
        public DynaUnionizer<V> Add(Dyna<V, object> vol, int3 offset)
        {
            _vols.Add((vol, offset));
            return this;
        }

        /// <summary>Add a volume only if <paramref name="maybe"/> has a value.</summary>
        public DynaUnionizer<V> MaybeAdd((Dyna<V, object> vol, int3 offset)? maybe)
        {
            if (maybe is (var v, var off))
                Add(v, off);
            return this;
        }

        /// <summary>Combine all volumes into one using identity mapping.</summary>
        public (Dyna<V, object> vol, int3 origin) Unify() => UnifyWith(v => v);

        /// <summary>
        /// Combine all volumes applying <paramref name="map"/> to each voxel.
        /// Returns the unified volume and the origin used for translation.
        /// </summary>
        public (Dyna<V, object> vol, int3 origin) UnifyWith(Func<V, V> map)
        {
            if (_vols.Count == 0)
            {
                return (Dyna<V, object>.Filled(int3.zero, _defaultNonFilled(), new object()), int3.zero);
            }

            int3 min = _vols[0].offset;
            int3 max = _vols[0].offset + _vols[0].vol.Size;
            for (int i = 1; i < _vols.Count; i++)
            {
                var off = _vols[i].offset;
                var size = _vols[i].vol.Size;
                min = math.min(min, off);
                max = math.max(max, off + size);
            }

            var newSize = max - min;
            var combined = Dyna<V, object>.Filled(newSize, _defaultNonFilled(), new object());
            var origin = -min;
            foreach (var (vol, off) in _vols)
            {
                foreach (var (pos, vox) in vol.Cells())
                {
                    if (_isFilled(vox))
                    {
                        combined.Set(origin + off + pos, map(vox));
                    }
                }
            }

            return (combined, origin);
        }
    }
}
